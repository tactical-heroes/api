using System.Net;
using System.Net.Http.Json;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Register;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Accounts.Register;

public sealed class RegisterAccountEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST accounts register should locate the account details endpoint")]
    public async Task PostRegister_Should_LocateAccountDetailsEndpoint_When_AccountIsCreated()
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/accounts/register",
            new RegisterAccountRequest(
                Email: "created@example.com",
                UserName: "created-account",
                Password: "StrongPassword1!"),
            JsonOptions,
            TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadFromJsonAsync<RegisterAccountResponse>(
            JsonOptions,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        responseBody.ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.AbsolutePath.ShouldBe(
            $"/api/v1/accounts/{responseBody.Id}");
    }
}
