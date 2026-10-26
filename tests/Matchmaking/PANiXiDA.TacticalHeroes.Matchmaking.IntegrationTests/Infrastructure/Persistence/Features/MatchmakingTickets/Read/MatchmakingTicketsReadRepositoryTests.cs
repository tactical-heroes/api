using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchmakingTickets.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Abstractions;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchmakingTickets.Read;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchmakingTicketsReadRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Matchmaking should find stored aggregate when persisted")]
    public async Task ExistsByIdAsync_Should_FindStoredAggregate_When_Persisted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Guid storedId;
        await using (var writeScope = fixture.CreateScope())
        {
            var ticket = Ticket();
            writeScope.ServiceProvider.GetRequiredService<IMatchmakingTicketsRepository>().Add(ticket);
            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(cancellationToken);
            storedId = ticket.Id.Value;
        }

        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchmakingTicketsReadRepository>();

        (await repository.ExistsByIdAsync(storedId, cancellationToken)).ShouldBeTrue();
        (await repository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken)).ShouldBeFalse();
        (await repository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "Matchmaking should return false when storage is empty")]
    public async Task AnyAsync_Should_ReturnFalse_When_StorageIsEmpty()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchmakingTicketsReadRepository>();

        (await repository.AnyAsync(cancellationToken)).ShouldBeFalse();
    }
}
