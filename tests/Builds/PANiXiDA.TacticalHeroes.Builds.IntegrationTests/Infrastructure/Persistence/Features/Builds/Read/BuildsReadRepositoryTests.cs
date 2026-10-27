using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Builds.Application.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Builds;

namespace PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Infrastructure.Persistence.Features.Builds.Read;

public sealed class BuildsReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Read repository should report persisted builds when builds exist")]
    public async Task ExistsByIdAsync_Should_ReportPersistedBuilds_When_BuildsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var build = BuildTestData.Create();
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IBuildsRepository>().Add(build);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var repository = verificationScope.ServiceProvider.GetRequiredService<IBuildsReadRepository>();

        (await repository.ExistsByIdAsync(build.Id.Value, cancellationToken)).ShouldBeTrue();
        (await repository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Read repository should detect builds when the first build is saved")]
    public async Task AnyAsync_Should_DetectBuilds_When_TheFirstBuildIsSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBuildsReadRepository>();
            (await repository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        var build = BuildTestData.Create();
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IBuildsRepository>().Add(build);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var populatedRepository = verificationScope.ServiceProvider.GetRequiredService<IBuildsReadRepository>();

        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }
}
