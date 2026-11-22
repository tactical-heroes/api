using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration;

internal static class MatchSearchExpirationTestSupport
{
    internal static async Task<MatchSearchStateChanged> WaitForExpiredAsync(
        IServiceProvider services, Guid searchId, CancellationToken ct)
    {
        var probe = services.GetRequiredService<MatchSearchStateChangedProbe>();
        var deadline = DateTimeOffset.UtcNow.AddSeconds(45);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var expired = probe.Events.FirstOrDefault(@event =>
                @event.MatchSearchId == searchId && @event.Status == "Expired");
            if (expired is not null)
            {
                return expired;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
        }

        throw new TimeoutException($"Expired state was not delivered for search {searchId}.");
    }
}
