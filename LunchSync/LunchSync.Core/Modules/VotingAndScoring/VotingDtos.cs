using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunchSync.Core.Modules.VotingAndScoring;
// ── Inbound 

/// <summary>POST /voting/{sessionId}/vote</summary>
public sealed record SubmitVoteRequest(
    /// <summary>8-char string, each char 'A' or 'B'.</summary>
    string Choices
);

// ── Outbound 

/// <summary>One binary choice as shown to the user.</summary>
public sealed record BinaryChoiceDto(
    int Index,
    string Label,
    string OptionA,
    string OptionB
);

/// <summary>Response after a participant submits their vote.</summary>
public sealed record VoteResultDto(
    int TotalVoted,
    int TotalParticipants,

    /// <summary>True when this vote triggered scoring (all participants voted).</summary>
    bool ScoringTriggered
);
