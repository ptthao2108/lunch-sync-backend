using LunchSync.Application.Caching;
using LunchSync.Core.Modules.Sessions.Entities;
using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.ValueObjects;
using StackExchange.Redis;
using System.Text.Json;
using LunchSync.Core.Exceptions;

namespace LunchSync.Infrastructure.Persistence.Caching;

public class SessionCache : ISessionCache
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private const int MaxRetries = 10;

    public SessionCache(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<Pin> GenerateUniquePinAsync(Guid sessionId, int expireMinutes)
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            var candidatePin = Pin.Generate();
            string key = RedisKeyBuilder.Data(candidatePin.Value);

            bool isReserved = await _db.StringSetAsync(
                key,
                sessionId.ToString(),
                TimeSpan.FromMinutes(expireMinutes),
                When.NotExists);

            if (isReserved) return candidatePin;
        }
        throw new BusinessRuleViolationException("Hệ thống hiện tại không thể tạo thêm mã PIN duy nhất.");
    }

    public async Task SaveActiveSessionAsync(Session session, int expireMinutes)
    {
        var key = RedisKeyBuilder.Data(session.Pin);
        var entries = new HashEntry[]
        {
        new("Id", session.Id.ToString()),
        new("HostId", session.HostId.ToString()),
        new("Status", (int)session.Status),
        new("CreatedAt", session.CreatedAt.ToString("O")),
        new("ExpiresAt", session.ExpiresAt?.ToString("O") ?? ""),
        new("CollectionId", session.CollectionId.ToString()),
        new("PriceTier", (int)session.PriceTier)
        };

        await _db.HashSetAsync(key, entries);

        var participantKey = RedisKeyBuilder.Participants(session.Pin);
        var nameKey = RedisKeyBuilder.Names(session.Pin);

        // Set TTL cho tất cả các key liên quan
        var ttl = TimeSpan.FromMinutes(expireMinutes);
        await _db.KeyExpireAsync(key, ttl);
        await _db.KeyExpireAsync(participantKey, ttl);
        await _db.KeyExpireAsync(nameKey, ttl);
    }

    public async Task<List<Participant>> GetParticipantsAsync(string pin)
    {
        var setKey = RedisKeyBuilder.Participants(pin);
        var members = await _db.SetMembersAsync(setKey); //lay participant trong set

        if (members == null || members.Length == 0)
            return new List<Participant>();

        return members
            .Select(m => JsonSerializer.Deserialize<Participant>(m!)) // duyet tung chuoi json trong redis va giai ma nguoc lai thanh participant
            .Where(x => x != null)
            .ToList()!;
    }

    public async Task<Session?> GetActiveSessionByPinAsync(string pin)
    {
        var key = RedisKeyBuilder.Data(pin);
        var data = await _db.HashGetAllAsync(key);
        if (data.Length == 0) return null;

        var dict = data.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

        return new Session
        {
            Id = dict.TryGetValue("Id", out var sId) ? Guid.Parse(sId) : Guid.Empty,
            Pin = pin,
            HostId = dict.TryGetValue("HostId", out var hId) ? Guid.Parse(hId) : Guid.Empty,
            Status = dict.TryGetValue("Status", out var st) ? Enum.Parse<SessionStatus>(st) : SessionStatus.Waiting,
            CreatedAt = dict.TryGetValue("CreatedAt", out var tc) ? DateTime.Parse(tc) : DateTime.UtcNow,
            ExpiresAt = dict.TryGetValue("ExpiresAt", out var te) ? DateTime.Parse(te) : DateTime.MinValue,
            CollectionId = dict.TryGetValue("CollectionId", out var cId) ? Guid.Parse(cId) : Guid.Empty,
            PriceTier = dict.TryGetValue("PriceTier", out var pt) ? Enum.Parse<PriceTier>(pt) : PriceTier.Under40k
        };
    }

    public async Task RemoveSessionAsync(string pin)
    {
        var keys = new RedisKey[]
        {
        RedisKeyBuilder.Names(pin),
        RedisKeyBuilder.Participants(pin),
        RedisKeyBuilder.Data(pin)
        };
        await _db.KeyDeleteAsync(keys);
    }

    public async Task UpdateStatusAndExpireAsync(string pin, SessionStatus newStatus, int expireMinutes)
    {
        var key = RedisKeyBuilder.Data(pin);
        var participantKey = RedisKeyBuilder.Participants(pin);
        var nameKey = RedisKeyBuilder.Names(pin);

        // 1. Cập nhật Status
        await _db.HashSetAsync(key, "Status", (int)newStatus);

        // 2. Gia hạn thời gian sống (TTL)
        var ttl = TimeSpan.FromMinutes(expireMinutes);
        await _db.KeyExpireAsync(key, ttl);
        await _db.KeyExpireAsync(participantKey, ttl);
        await _db.KeyExpireAsync(nameKey, ttl);
    }

    public async Task<int> TryJoinAtomicAsync(string pin, Participant participant, int maxParticipants, int expireMinutes)
    {

        var participantJson = JsonSerializer.Serialize(participant);
        var NickName = participant.Nickname.ToLower().Trim();

        var keys = new RedisKey[]
        {
        RedisKeyBuilder.Names(pin), //KEYS[1]
        RedisKeyBuilder.Participants(pin), //KEYS[2]
        RedisKeyBuilder.Data(pin) //KEYS[3]
        };

        string luaScript = @"
            -- 0. Check if session exists
            if redis.call('EXISTS', KEYS[3]) == 0 then return 3 end
            
            -- 1. check duplicated name
            if redis.call('SISMEMBER', KEYS[1], ARGV[1]) == 1 then return 1 end 

            -- 2. Check count
            local currentCount = tonumber(redis.call('SCARD', KEYS[2]) or '0')
            if currentCount >= tonumber(ARGV[2]) then return 2  end

            -- 3. Add (Atomic)
            redis.call('SADD', KEYS[1], ARGV[1]) -- Thêm tên
            redis.call('SADD', KEYS[2], ARGV[3]) -- participant

            -- 4. Set TTL
            local ttl = tonumber(ARGV[4])
            for i, key in ipairs(KEYS) do
                redis.call('EXPIRE', key, ttl)
            end

            return 0 -- Thành công
        ";

        var result = await _db.ScriptEvaluateAsync(luaScript, keys,
            new RedisValue[] { NickName, maxParticipants, participantJson, expireMinutes * 60 });
        // ARG[1]   ARG[2]           ARG[3]           ARG[4]

        return (int)result;
    }

}