using Microsoft.Extensions.DependencyInjection;

using Wolverine.Runtime;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

[Collection(RolesFunctionalTestCollectionDefinition.Name)]
public sealed class HostSchedulingTests(FunctionalTestFixture fixture)
{
    [Theory(DisplayName = "Scheduling should disable foreign schedules when identity host starts")]
    [InlineData("Test")]
    [InlineData("Development")]
    public async Task Scheduling_Should_DisableForeignSchedules_When_IdentityHostStarts(string environment)
    {
        await using var factory = fixture.CreateFactory(environment);
        var runtime = factory.Services.GetRequiredService<IWolverineRuntime>();

        var schedules = runtime.Options.Schedules;

        schedules.FindByName("file-manager-reconcile-uploads").ShouldBeNull();
        schedules.FindByName("matchmaking-process").ShouldBeNull();
        schedules.FindByName("matchmaking-expire-searches").ShouldBeNull();
    }
}
