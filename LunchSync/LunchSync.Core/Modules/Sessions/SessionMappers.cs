using LunchSync.Core.Modules.Sessions.Entities;
using LunchSync.Core.Modules.Sessions;

namespace LunchSync.Core.Modules.Sessions;

public static class SessionsMappers
{
    public static CreateSessionRes ToCreateSessionRes(this Session session, string collectionName, string baseUrl)
    {
        return new CreateSessionRes
        {
            SessionId = session.Id,
            Pin = session.Pin,
            ShareLink = $"{baseUrl}/{session.Pin}",
            Status = session.Status.ToString().ToLower(),
            CollectionName = collectionName,
            participantId = session.HostId,
        };
    }
    public static JoinRes ToJoinRes(this Participant participant, Session session)
    {
        return new JoinRes
        {
            ParticipantId = participant.Id,
            SessionId = session.Id,
            Nickname = participant.Nickname,
            Participants = session.Participants.Select(p => new ParticipantDto
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
            VotingStartedAt = session.VotingStartedAt
        };
    }
    public static SessionInfoDto ToInfoDto(this Session session, int votedCount)
    {
        return new SessionInfoDto
        {
            SessionId = session.Id,
            Pin = session.Pin,
            Status = session.Status.ToString().ToLower(),
            HostName = session.Participants
                        .FirstOrDefault(p => p.UserId == session.HostId)?
                        .Nickname ?? "Unknown Host",
            CollectionName = session.Collection.Name,
            PriceTier = session.PriceTier.ToString(),
            PriceDisplay = session.PriceTier.ToString() + "/phần",
            Participants = session.Participants.Select(p => new ParticipantDto
            {
                Nickname = p.Nickname,
                JoinedAt = p.JoinedAt,
                IsHost = p.UserId != null && p.UserId == session.HostId,
            }).ToList(),
            ParticipantCount = session.Participants.Count,
            CreatedAt = session.CreatedAt,
            ExpiredAt = session.ExpiresAt
        };
    }
    public static SessionStartRes ToStartRes(this Session session)
    {
        return new SessionStartRes
        {
            Status = session.Status.ToString().ToLower(),
            ParticipantCount = session.Participants.Count,
            VotingStartedAt = session.VotingStartedAt,
            Message = $"Bắt đầu bỏ phiếu vs {session.Participants.Count} người"
        };
    }
    public static SessionCloseVotingRes ToCloseVotingRes(this Session session, int votedCount)
    {
        return new SessionCloseVotingRes
        {
            Status = session.Status.ToString().ToLower(),
            TotalParticipants = session.Participants.Count,
            TotalVoted = votedCount,
            Message = $"Scoring với {votedCount} người đã vote"
        };
    }
}
