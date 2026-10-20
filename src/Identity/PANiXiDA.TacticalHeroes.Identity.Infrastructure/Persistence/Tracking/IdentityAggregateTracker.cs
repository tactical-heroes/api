using PANiXiDA.Core.Infrastructure.Persistence.Ef.Tracking;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Tracking;

internal sealed class IdentityAggregateTracker(AggregateTracker efTracker) : IAggregateTracker
{
    private readonly HashSet<IAggregateRoot> _aggregates = new(ReferenceEqualityComparer.Instance);

    public void Track(IAggregateRoot aggregateRoot)
    {
        if (aggregateRoot is User or Role)
        {
            _aggregates.Add(aggregateRoot);
        }
        else
        {
            efTracker.Track(aggregateRoot);
        }
    }

    public IReadOnlyCollection<IAggregateRoot> GetAll()
    {
        return [.. efTracker.GetAll(), .. _aggregates];
    }

    public void Clear()
    {
        efTracker.Clear();
        _aggregates.Clear();
    }
}
