using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Tracking;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchSearchStateVersionInterceptorTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "State stream should assign state version when saving synchronously")]
    public void SaveChanges_Should_AssignStateVersion_When_SavingSynchronously()
    {
        using var scope = fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
        var search = CreateSearch();
        context.Add(search);

        context.SaveChanges();

        context.Entry(search).Property<long>("StateVersion").CurrentValue.ShouldBeGreaterThan(0);
    }
}
