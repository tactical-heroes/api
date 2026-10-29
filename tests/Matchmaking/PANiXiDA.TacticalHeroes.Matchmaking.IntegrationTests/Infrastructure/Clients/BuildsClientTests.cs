using PANiXiDA.TacticalHeroes.Builds.Contracts.Builds;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Clients;

public sealed class BuildsClientTests
{
    [Fact(DisplayName = "Client should map contract build when build exists")]
    public async Task GetByIdAsync_Should_MapContractBuild_When_BuildExists()
    {
        var build = new BuildResponse(Guid.CreateVersion7(), Guid.CreateVersion7());
        var api = new StubBuildsApi(build);
        var client = new BuildsClient(api);
        var ct = TestContext.Current.CancellationToken;

        var result = await client.GetByIdAsync(build.Id, ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(build.Id);
        result.PlayerId.ShouldBe(build.PlayerId);
        api.Id.ShouldBe(build.Id);
        (api.CancellationToken == ct).ShouldBeTrue();
    }

    [Fact(DisplayName = "Client should return null when module returns no build")]
    public async Task GetByIdAsync_Should_ReturnNull_When_ModuleReturnsNoBuild()
    {
        var api = new StubBuildsApi(null);
        var client = new BuildsClient(api);

        var result = await client.GetByIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    private sealed class StubBuildsApi(BuildResponse? result) : IBuildsApi
    {
        public Guid Id { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<BuildResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Id = id;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
