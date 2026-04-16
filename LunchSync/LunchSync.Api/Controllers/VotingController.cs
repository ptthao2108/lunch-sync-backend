using LunchSync.Core.Modules.VotingAndScoring;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LunchSync.Api.Controllers;

[ApiController]
[Route("api/sessions/{sessionId:guid}/voting")]
[Authorize]
public sealed class VotingController : ControllerBase
{
    private readonly IVotingService _votingService;

    public VotingController(IVotingService votingService)
        => _votingService = votingService;

    /// <summary>GET the 8 binary choice questions.</summary>
    [HttpGet("choices")]
    [AllowAnonymous]   // guests can also see the questions
    public IActionResult GetChoices()
    {
        var choices = _votingService.GetChoices();
        return Ok(choices);
    }

    /// <summary>POST a participant's 8-character answer string.</summary>
    [HttpPost("vote")]
    public async Task<IActionResult> SubmitVote(
        Guid sessionId,
        [FromBody] SubmitVoteRequest request,
        CancellationToken ct)
    {
        // participantId resolved from JWT claim
        var participantId = GetParticipantId();
        var result = await _votingService.SubmitVoteAsync(sessionId, participantId, request.Choices, ct);
        return Ok(result);
    }

    /// <summary>POST by host to close voting early (lazy evaluation).</summary>
    [HttpPost("close")]
    public async Task<IActionResult> CloseVoting(Guid sessionId, CancellationToken ct)
    {
        var hostUserId = GetUserId();
        await _votingService.CloseVotingAsync(sessionId, hostUserId, ct);
        return NoContent();
    }

    // ── Helpers 

    private Guid GetParticipantId()
    {
        var claim = User.FindFirst("participant_id")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("participant_id claim missing.");
        return Guid.Parse(claim);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("sub claim missing.");
        return Guid.Parse(claim);
    }
}
