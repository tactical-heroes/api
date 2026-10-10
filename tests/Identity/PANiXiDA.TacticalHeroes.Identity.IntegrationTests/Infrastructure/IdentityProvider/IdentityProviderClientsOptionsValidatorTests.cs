using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Clients;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.IdentityProvider;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider;

public sealed class IdentityProviderClientsOptionsValidatorTests
{
    [Theory(DisplayName = "Client validation should permit introspection only for confidential clients when grant types are empty")]
    [InlineData(OpenIddictConstants.ClientTypes.Confidential, true)]
    [InlineData(OpenIddictConstants.ClientTypes.Public, false)]
    public void Validate_Should_AllowOnlyConfidentialClients_When_GrantTypesAreEmpty(string clientType, bool expectedSuccess)
    {
        var options = new IdentityProviderOptions
        {
            Clients =
            [
                new IdentityProviderClientOptions
                {
                    ClientId = "resource-api",
                    ClientType = clientType,
                    ClientSecret = "test-introspection-secret",
                    DisplayName = "Resource API"
                }
            ]
        };
        var validator = new IdentityProviderClientsOptionsValidator();

        var result = validator.Validate(null, options);

        result.Succeeded.ShouldBe(expectedSuccess);
        if (!expectedSuccess)
        {
            result.Failures.ShouldNotBeNull();
            result.Failures.ShouldContain("Identity:Provider:Clients:0:GrantTypes must contain at least one value.");
        }
    }
}
