using LunchSync.Core.Modules.Sessions.Entities;
using LunchSync.Core.Modules.Sessions;

namespace LunchSync.Core.Modules.Sessions;

public static class SessionsMappers
{
    public static CreateSessionRes ToCreateSessionRes(this Session session, string collectionName, string shareLink)
    {
        return new CreateSessionRes
        {
            SessionId = session.Id,
            Pin = session.Pin,
            ShareLink = shareLink,
            Status = session.Status.ToString().ToLower(),
            CollectionName = collectionName
        };
    }
    public static JoinRes ToJoinRes(this Participant participant, Session session)
    {
        return new JoinRes
        {
            ParticipantId = participant.Id,
            SessionId = session.Id,
            Nickname = participant.Nickname,
            Participants = session.Participants.Select(p => new Participant
            {
                Nickname = p.Nickname,
                JoinedAt = p.JoinedAt
            }).ToList()
        };
    }
    public static SessionStatusDto ToStatusDto(this Session session, int votedCount)
    {
        return new SessionStatusDto
        {
            Status = session.Status.ToString().ToLower(),
            ParticipantsJoined = session.Participants.Count,
            ParticipantsVoted = votedCount,
            VotingStartedAt = session.VotingStartedAt,
            BoomTriggeredAt = session.BoomTriggeredAt
        };
    }
}
