using System.Net;
using System.Net.Http.Json;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Auth.Register;

public sealed class RegisterUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST auth register should locate the user details endpoint")]
    public async Task PostRegister_Should_LocateUserDetailsEndpoint_When_UserIsCreated()
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest(
                Email: "created@example.com",
                UserName: "created-user",
                Password: "StrongPassword1!"),
            JsonOptions,
            TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(
            JsonOptions,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        responseBody.ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.AbsolutePath.ShouldBe(
            $"/api/v1/users/{responseBody.Id}");
    }
}
