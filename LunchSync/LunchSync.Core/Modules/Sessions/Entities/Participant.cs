using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstactions;
namespace LunchSync.Core.Modules.Sessions.Entities;

public sealed class Participant: BaseEntity
{
    public Guid SessionId { get; set;} = null!;
    
    public string Nickname { get; set; } = null!;
    //unique(SessionId, Nickname)

    public List<float>? PrefVector { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VotedAt { get; set; }

    //navigation
    public Session Session{ get; set; } = null!;
}