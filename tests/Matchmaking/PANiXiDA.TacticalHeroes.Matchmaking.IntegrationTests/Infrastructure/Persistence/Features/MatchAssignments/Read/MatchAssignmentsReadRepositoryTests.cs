using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchAssignments.Abstractions;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchAssignments.Read;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchAssignmentsReadRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Matchmaking should find stored aggregate when persisted")]
    public async Task ExistsByIdAsync_Should_FindStoredAggregate_When_Persisted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Guid storedId;
        await using (var writeScope = fixture.CreateScope())
        {
            var assignment = await SaveProposalAsync(writeScope.ServiceProvider, cancellationToken);
            storedId = assignment.Id.Value;
        }

        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchAssignmentsReadRepository>();

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
        var repository = scope.ServiceProvider.GetRequiredService<IMatchAssignmentsReadRepository>();

        (await repository.AnyAsync(cancellationToken)).ShouldBeFalse();
    }
}
