
using LunchSync.Core.Common.Enums;
using LunchSync.Core.Modules.Sessions;

using Microsoft.Extensions.Configuration;

namespace LunchSync.Core.Modules.Sessions;

public class SessionService : ISessionService
{
    private readonly ISessionCache _cache;
    private readonly ISessionRepository _repository;
    private readonly IConfiguration _configuration;
    private const int MaxParticipants = 8;
    private const int MinParticipants = 3;
    private const int DefaultExpiryMinutes = 30;

    public SessionService(ISessionCache cache, ISessionRepository repository, IConfiguration configuration)
    {
        _cache = cache;
        _repository = repository;
        _configuration = configuration;
    }

    public Task<CreateSessionRes> CreateSessionAsync(CreateSessionReq request, Guid HostId) => throw new NotImplementedException();
    public Task<JoinRes> JoinSessionAsync(string pin, JoinReq request) => throw new NotImplementedException();
    public Task<SessionStatusDto> StartSessionAsync(string pin) => throw new NotImplementedException();
    public Task<SessionStatusDto> CancelSessionAsync(string pin, Guid hostId) => throw new NotImplementedException();
    public Task<SessionStatusDto> CompleteSessionAsync(string pin) => throw new NotImplementedException();

}
