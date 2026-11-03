using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Tracking;

internal sealed class MatchSearchStateVersionInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is MatchmakingWriteDbContext context)
        {
            var entries = GetChangedSearches(context);
            if (entries.Length > 0)
            {
                var versions = context.Database.SqlQuery<long>(
                    $"SELECT nextval('matchmaking.match_search_state_versions') AS \"Value\" FROM generate_series(1, {entries.Length})")
                    .ToArray();
                AssignVersions(entries, versions);
            }
        }

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken)
    {
        if (eventData.Context is MatchmakingWriteDbContext context)
        {
            var entries = GetChangedSearches(context);
            if (entries.Length > 0)
            {
                var versions = await context.Database.SqlQuery<long>(
                    $"SELECT nextval('matchmaking.match_search_state_versions') AS \"Value\" FROM generate_series(1, {entries.Length})")
                    .ToArrayAsync(cancellationToken);
                AssignVersions(entries, versions);
            }
        }

        return result;
    }

    private static EntityEntry<MatchSearch>[] GetChangedSearches(MatchmakingWriteDbContext context)
    {
        return context.ChangeTracker.Entries<MatchSearch>()
            .Where(entry => entry.State == EntityState.Added
                || entry.State == EntityState.Modified
                    && (Changed(entry.Property(search => search.Status))
                        || Changed(entry.Property(search => search.GameId))))
            .ToArray();
    }

    private static bool Changed(PropertyEntry property)
    {
        return !Equals(property.OriginalValue, property.CurrentValue);
    }

    private static void AssignVersions(EntityEntry<MatchSearch>[] entries, long[] versions)
    {
        for (var index = 0; index < entries.Length; index++)
        {
            entries[index].Property<long>("StateVersion").CurrentValue = versions[index];
        }
    }
}
