using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.Players.Domain.Files;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

using Wolverine.Runtime;
using Wolverine.Tracking;

using IdentityDomainEvent = PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events.UserRegistered;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.UserRegistered;

namespace PANiXiDA.TacticalHeroes.Players.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class UserRegistrationTests(FunctionalTestFixture fixture)
{
    private static readonly TimeSpan s_trackingTimeout = TimeSpan.FromSeconds(30);

    [Fact(DisplayName = "Scheduling should disable foreign schedules when only players schema is migrated")]
    public void Scheduling_Should_DisableForeignSchedules_When_OnlyPlayersSchemaIsMigrated()
    {
        var runtime = fixture.Services.GetRequiredService<IWolverineRuntime>();

        var schedule = runtime.Options.Schedules.FindByName("matchmaking-expire-searches");

        schedule.ShouldBeNull();
        runtime.Options.Schedules.FindByName("file-manager-reconcile-uploads").ShouldBeNull();
    }

    [Fact(DisplayName = "User registration should create a player through both mappings when user is registered")]
    public async Task PublishAsync_Should_CreatePlayerThroughBothMappings_When_UserIsRegistered()
    {
        var userId = UserId.New();
        var domainEvent = new IdentityDomainEvent(userId.Value, "hero@example.com");
        var ct = TestContext.Current.CancellationToken;

        var tracked = await fixture.Services.TrackActivity()
            .Timeout(s_trackingTimeout)
            .PublishMessageAndWaitAsync(domainEvent);

        var integrationEvent = tracked.FindSingleTrackedMessageOfType<IntegrationEvent>();
        integrationEvent.Id.ShouldBe(domainEvent.Id);
        integrationEvent.UserId.ShouldBe(userId.Value);
        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PlayersWriteDbContext>();
        var player = await context.Set<Player>().AsNoTracking().SingleAsync(item => item.UserId == userId, ct);
        player.Id.Value.ShouldNotBe(Guid.Empty);
        player.Rating.Value.ShouldBe(1000);
        player.AvatarFileId.ShouldBeNull();
    }

    [Theory(DisplayName = "User registration should preserve the player profile when registration is delivered again")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PublishAsync_Should_PreservePlayerProfile_When_RegistrationIsDeliveredAgain(bool sameEvent)
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = UserId.New();
        var integrationEvent = new IntegrationEvent(userId.Value);
        await fixture.Services.TrackActivity()
            .Timeout(s_trackingTimeout)
            .PublishMessageAndWaitAsync(integrationEvent);
        var avatarFileId = FileId.New();
        Guid playerId;
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PlayersWriteDbContext>();
            var player = await context.Set<Player>().SingleAsync(item => item.UserId == userId, ct);
            playerId = player.Id.Value;
            player.ChangeAvatar(avatarFileId);
            player.ChangeRating(Rating.Create(1700));
            scope.ServiceProvider.GetRequiredService<IPlayersRepository>().Update(player);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
                .SaveChangesAsync(ct);
        }

        await fixture.Services.TrackActivity()
            .Timeout(s_trackingTimeout)
            .PublishMessageAndWaitAsync(sameEvent ? integrationEvent : new IntegrationEvent(userId.Value));

        await using var verificationScope = fixture.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<PlayersWriteDbContext>();
        var restored = await verificationContext.Set<Player>().AsNoTracking()
            .SingleAsync(item => item.UserId == userId, ct);
        restored.Id.Value.ShouldBe(playerId);
        restored.Rating.Value.ShouldBe(1700);
        restored.AvatarFileId.ShouldBe(avatarFileId);
    }
}
