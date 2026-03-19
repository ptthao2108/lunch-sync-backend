using LunchSync.Core.Modules.Sessions.Entities;
using LunchSync.Core.Common.Enums;
namespace LunchSync.Core.Modules.Sessions;
public interface ISessionRepository
{
    Task<Guid> SaveHistoryAsync(Session Session);
    Task<Session?> GetHistoryByIdAsync(Guid SessionId);
    Task<List<Session>> GetSessionsToExpireAsync(DateTime threshold);
    Task UpdateSessionsStatusAsync(List<Guid> SessionIds, SessionStatus status);
    Task<Session?> GetLastSessionByHostIdAsync(Guid hostId);
}
