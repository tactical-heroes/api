using PANiXiDA.TacticalHeroes.Builds.Application.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Application.Builds.GetInternalBuild;

public sealed class GetInternalBuildHandlerTests
{
    [Fact(DisplayName = "Handler should return stored build when build exists")]
    public async Task HandleAsync_Should_ReturnStoredBuild_When_BuildExists()
    {
        var build = new BuildReadModel(Guid.CreateVersion7(), Guid.CreateVersion7());
        var repository = Substitute.For<IBuildsReadRepository>();
        var ct = TestContext.Current.CancellationToken;
        repository.GetByIdAsync(build.Id, ct).Returns(build);
        var handler = new GetInternalBuildHandler(repository);

        var result = await handler.HandleAsync(new GetInternalBuildQuery(build.Id), ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(build);
        await repository.Received(1).GetByIdAsync(build.Id, ct);
    }

    [Fact(DisplayName = "Handler should return not found when build is missing")]
    public async Task HandleAsync_Should_ReturnNotFound_When_BuildIsMissing()
    {
        var repository = Substitute.For<IBuildsReadRepository>();
        var handler = new GetInternalBuildHandler(repository);

        var result = await handler.HandleAsync(
            new GetInternalBuildQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }
}
