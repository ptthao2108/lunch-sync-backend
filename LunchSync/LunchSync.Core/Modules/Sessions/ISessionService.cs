using LunchSync.Core.Modules.Sessions.Entities;

namespace LunchSync.Core.Modules.Sessions;

public interface ISessionService
{
    Task<CreateSessionRes> CreateSessionAsync(CreateSessionReq request, Guid HostId);
    Task<JoinRes> JoinSessionAsync(string pin, JoinReq request);
    Task<SessionStatusDto> StartSessionAsync(string pin);
    Task<SessionStatusDto> CancelSessionAsync(string pin, Guid hostId);
    Task<SessionStatusDto> CompleteSessionAsync(string pin);
}
