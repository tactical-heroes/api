using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddInfrastructure should resolve identity ports")]
    public async Task AddInfrastructure_Should_ResolveIdentityPorts()
    {
        await using var scope = Fixture.CreateScope();

        scope.ServiceProvider.GetRequiredService<IUsersReadRepository>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IUsersWriteRepository>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IUserCredentialsService>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IRolesReadRepository>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IOAuthUsersRepository>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IOAuthClientsRepository>().ShouldNotBeNull();
    }

    [Fact(DisplayName = "AddInfrastructure should configure native OpenIddict endpoints")]
    public async Task AddInfrastructure_Should_ConfigureNativeOpenIddictEndpoints()
    {
        await using var scope = Fixture.CreateScope();
        var serverOptions = scope.ServiceProvider
            .GetRequiredService<IOptionsMonitor<OpenIddictServerOptions>>()
            .CurrentValue;
        var aspNetCoreOptions = scope.ServiceProvider
            .GetRequiredService<IOptionsMonitor<OpenIddictServerAspNetCoreOptions>>()
            .CurrentValue;

        serverOptions.PushedAuthorizationEndpointUris
            .Select(uri => uri.OriginalString)
            .ShouldContain("/connect/par");
        serverOptions.IntrospectionEndpointUris
            .Select(uri => uri.OriginalString)
            .ShouldContain("/connect/introspect");
        serverOptions.RevocationEndpointUris
            .Select(uri => uri.OriginalString)
            .ShouldContain("/connect/revoke");
        serverOptions.RefreshTokenReuseLeeway.ShouldBe(TimeSpan.FromSeconds(seconds: 5));
        aspNetCoreOptions.EnableAuthorizationEndpointPassthrough.ShouldBeTrue();
        aspNetCoreOptions.EnableTokenEndpointPassthrough.ShouldBeTrue();
        aspNetCoreOptions.EnableUserInfoEndpointPassthrough.ShouldBeTrue();
        aspNetCoreOptions.EnableEndSessionEndpointPassthrough.ShouldBeTrue();
    }
}
