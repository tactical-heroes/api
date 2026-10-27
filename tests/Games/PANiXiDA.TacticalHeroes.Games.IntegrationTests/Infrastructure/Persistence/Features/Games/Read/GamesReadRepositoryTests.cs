using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Games.Application.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Games.IntegrationTests.Games;

namespace PANiXiDA.TacticalHeroes.Games.IntegrationTests.Infrastructure.Persistence.Features.Games.Read;

public sealed class GamesReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Read repository should report persisted games when games exist")]
    public async Task ExistsByIdAsync_Should_ReportPersistedGames_When_GamesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var game = GameTestData.Create();
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IGamesRepository>().Add(game);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(GamesWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var repository = verificationScope.ServiceProvider.GetRequiredService<IGamesReadRepository>();

        (await repository.ExistsByIdAsync(game.Id.Value, cancellationToken)).ShouldBeTrue();
        (await repository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Read repository should detect games when the first game is saved")]
    public async Task AnyAsync_Should_DetectGames_When_TheFirstGameIsSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IGamesReadRepository>();
            (await repository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        var game = GameTestData.Create();
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IGamesRepository>().Add(game);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(GamesWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var populatedRepository = verificationScope.ServiceProvider.GetRequiredService<IGamesReadRepository>();

        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }
}
