using LunchSync.Core.Common.Enums;
using LunchSync.Core.Modules.Sessions.Entities;

namespace LunchSync.Core.Modules.Sessions;

public class CreateSessionReq
{
    public string? Hostname { get; set; }
    public Guid CollectionId { get; set; }
    public PriceTier PriceTier { get; set; }
}
public class CreateSessionRes
{
    public Guid SessionId { get; set; }
    public string Pin { get; set; } = string.Empty;
    public string ShareLink { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
}

public class JoinReq
{
    public string Pin { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
}
public class JoinRes
{
    public Guid ParticipantId { get; set; }
    public Guid SessionId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public List<Participant> Participants { get; set; } = new List<Participant>();
}
public class SessionStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int ParticipantsJoined { get; set; }
    public int ParticipantsVoted { get; set; }
    public DateTime? VotingStartedAt { get; set; }
    public DateTime? BoomTriggeredAt { get; set; }
}
