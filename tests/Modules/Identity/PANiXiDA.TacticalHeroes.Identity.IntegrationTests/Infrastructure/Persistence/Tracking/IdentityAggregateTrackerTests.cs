using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.Domain.AggregateRoots;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Tracking;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Tracking;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Tracking;

public sealed class IdentityAggregateTrackerTests
{
    [Theory(DisplayName = "Tracker should collect and clear Identity and EF roots when adapters share scope")]
    [InlineData(false)]
    [InlineData(true)]
    public void GetAll_Should_CombineIdentityAndEfRoots_When_AdaptersShareScope(bool trackRole)
    {
        using var first = new TrackerDbContext();
        using var second = new TrackerDbContext();
        using var efTracker = new AggregateTracker([() => first, () => second]);
        var tracker = new IdentityAggregateTracker(efTracker);
        IAggregateRoot identityRoot = trackRole
            ? Role.Create(RoleId.New(), RoleName.Create("Player").Value, RoleDefaultFlag.Create(false).Value, [])
            : User.Create(
                UserId.New(),
                Email.Create("hero@example.com").Value,
                UserName.Create("hero").Value,
                UserStatus.Active,
                UserConfirmationStatus.Unconfirmed(),
                [],
                []);
        var firstRoot = new TrackedAggregate(UserId.New());
        var secondRoot = new TrackedAggregate(UserId.New());
        first.Add(firstRoot);
        second.Add(secondRoot);

        tracker.Track(identityRoot);
        tracker.Track(identityRoot);
        tracker.Track(firstRoot);
        var snapshot = tracker.GetAll();
        tracker.Clear();

        snapshot.Count.ShouldBe(3);
        snapshot.ShouldContain(identityRoot);
        snapshot.ShouldContain(firstRoot);
        snapshot.ShouldContain(secondRoot);
        tracker.GetAll().ShouldBeEmpty();
        first.Entry(firstRoot).State.ShouldBe(EntityState.Added);
        second.Entry(secondRoot).State.ShouldBe(EntityState.Added);

        tracker.Track(identityRoot);
        tracker.Track(firstRoot);

        tracker.GetAll().Count.ShouldBe(2);
        tracker.GetAll().ShouldContain(identityRoot);
        tracker.GetAll().ShouldContain(firstRoot);
    }

    private sealed class TrackedAggregate(UserId id) : AggregateRoot<UserId>(id);

    private sealed class TrackerDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=tracker_model");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackedAggregate>()
                .Property(aggregate => aggregate.Id)
                .HasConversion(id => id.Value, value => UserId.Create(value).Value);
        }
    }
}
