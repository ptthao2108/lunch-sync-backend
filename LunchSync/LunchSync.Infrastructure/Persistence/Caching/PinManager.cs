using LunchSync.Core.Modules.Sessions;
using LunchSync.Core.Modules.Sessions.Entities;
using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.ValueObjects;
using StackExchange.Redis;
using System.Text.Json;
using LunchSync.Core.Exceptions;

namespace LunchSync.Infrastructure.Persistence.Caching;

public class PinManager : IPinManager
{
    private readonly IDatabase _db;
    private const int MaxGenRetries = 10;
    private const int LockSeconds = 30; // Thời gian giữ chỗ tạm thời

    public PinManager(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<string> GetUnusedPinAsync()
    {
        var poolKey = RedisKeyBuilder.PinAvailablePool();
        //Lấy từ pool: SPOP (lấy PIN) -> Nếu có thì SET session:{pin}:data "reserved" EX 30
        string luaScript = @"
            local pin = redis.call('SPOP', KEYS[1])
            if pin then
                local sessionKey = 'session:' .. pin .. ':data'
                redis.call('SET', sessionKey, 'reserved', 'EX', ARGV[1])
                return pin
            end
            return nil"; //null

        var result = await _db.ScriptEvaluateAsync(luaScript,
            new RedisKey[] { poolKey },
            new RedisValue[] { LockSeconds });

        if (!result.IsNull)
        {
            return result.ToString();
        }
        //Pool rỗng => gen mới
        for (int i = 0; i < MaxGenRetries; i++)
        {
            var candidatePin = Pin.Generate().Value;
            var sessionKey = RedisKeyBuilder.Data(candidatePin);

            bool isReserved = await _db.StringSetAsync(
                sessionKey,
                "reserved",
                TimeSpan.FromSeconds(LockSeconds),
                When.NotExists); //Chỉ đặt nếu key chưa tồn tại, k đè lên Session đang Active hoặc đang Reserved

            if (isReserved)
            { return candidatePin; }
        }
        throw new BusinessRuleViolationException("Hệ thống hiện tại không thể tạo thêm mã PIN duy nhất.");
    }
    public async Task<bool> ReleasePinAsync(string pin)
    {
        var sessionKey = RedisKeyBuilder.Data(pin); // session:{pin}:data
        var poolKey = RedisKeyBuilder.PinAvailablePool();
        //Check status
        // KEYS[1]: session data key
        // KEYS[2]: pin pool key
        // ARGV[1]: Mã PIN cần release
        // ARGV[2]: Giá trị enum của 'Done'
        // ARGV[3]: Giá trị enum của 'Cancelled'  - or status == ARGV[3] , (int)SessionStatus.Cancelled
        string luaScript = @"
            local status = redis.call('HGET', KEYS[1], 'Status')
            if status == ARGV[2]  then
                -- Nếu đã Done hoặc Cancelled, đẩy vào Pool
                redis.call('SADD', KEYS[2], ARGV[1])
                return 1 -- Thành công
            end
            return 0 -- Không thỏa mãn điều kiện để release
        ";
        var result = await _db.ScriptEvaluateAsync(luaScript,
            new RedisKey[] { sessionKey, poolKey },
            new RedisValue[] { pin, (int)SessionStatus.Done });

        return (int)result == 1;
    }
}
