using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Interfaces;
using LunchSync.Core.Exceptions;
using LunchSync.Core.Modules.VotingAndScoring.Config;

namespace LunchSync.Core.Modules.VotingAndScoring.Scoring;

/// <summary>
/// Orchestrates the full scoring pipeline once all participants have voted.
/// Scoped lifetime.
/// </summary>
public sealed class SessionScoringService
{
    private readonly IUnitOfWork _uow;
    private readonly IDishProfileCache _dishCache;
    private readonly ScoringEngine _engine;
    private readonly RestaurantMatcher _matcher;

    public SessionScoringService(
        IUnitOfWork uow,
        IDishProfileCache dishCache,
        ScoringEngine engine,
        RestaurantMatcher matcher)
    {
        _uow = uow;
        _dishCache = dishCache;
        _engine = engine;
        _matcher = matcher;
    }

    public async Task RunAsync(Guid sessionId, CancellationToken ct = default)
    {
        // ── 1. Load session ───────────────────────────────────────────────────
        var session = await _uow.Sessions.GetSessionByIdAsync(sessionId, ct)
                ?? throw new SessionNotFoundByIdException(sessionId);

        if (session.Status != SessionStatus.Voting)
            throw new BusinessRuleViolationException("Session is not in Voting state.");


        // ── 2. Collect individual vectors ────────────────────────────────────
        // Participant.PrefVector is List<float>? — convert to double[] for the engine
        var vectors = session.Participants
            .Where(p => p.PrefVector is { Count: > 0 })
            .Select(p => p.PrefVector!.Select(f => (double)f).ToArray())
            .ToList();

        if (vectors.Count == 0)
            throw new BusinessRuleViolationException("No participant vectors found.");

        // ── 3. Group aggregation + spicy veto ────────────────────────────────
        double[] effectiveVector = _engine.AggregateGroupVector(vectors);

        // ── 4. Available dish IDs for this collection (spec §6.4) ────────────
        var availableDishIds = await _uow.Dishes
            .GetAvailableDishIdsByCollectionAsync(session.CollectionId, ct);

        var availableSet = availableDishIds.ToHashSet();

        // ── 5. Score dishes (spec §6.5) ──────────────────────────────────────
        var allProfiles = _dishCache.GetAll();   // in-memory, ~0 ms
        var scored = _engine.ScoreDishes(effectiveVector, allProfiles, availableSet);

        var top7Ids = scored
            .Take(ScoringConstants.TopDishesInternal)
            .Select(x => x.DishId)
            .ToList();

        var top3Ids = scored
            .Take(ScoringConstants.TopDishesForUi)
            .Select(x => x.DishId)
            .ToList();

        // ── 6. Restaurant matching (spec §6.6) ───────────────────────────────
        var restaurantRows = await _uow.Restaurants
            .GetRestaurantsByTopDishesAsync(session.CollectionId, top7Ids, ct);

        var candidates = restaurantRows.Select(r => new RestaurantCandidate(
            r.Id,
            r.PriceTier,
            r.Rating,
            r.RestaurantDishes.Select(rd => rd.DishId).ToList()
        ));

        var top5 = _matcher.MatchTopRestaurants(candidates, top7Ids, session.PriceTier);

        var top5Ids = top5.Select(x => x.RestaurantId).ToList();

        // ── 7. Persist (spec §6.7) — single attach + targeted updates ────────
        // Session was loaded AsNoTracking, so we attach and mark only changed columns.

        // GroupVector stored as List<float> on the entity
        var groupVectorAsFloat = effectiveVector.Select(d => (float)d).ToList();

        await _uow.Sessions.UpdateSessionAsync(session, s => s.GroupVector, groupVectorAsFloat);
        await _uow.Sessions.UpdateSessionAsync(session, s => s.TopDishIds, top3Ids);
        await _uow.Sessions.UpdateSessionAsync(session, s => s.TopRestaurantIds, top5Ids);
        await _uow.Sessions.UpdateSessionAsync(session, s => s.Status, SessionStatus.Results);
    }
}
