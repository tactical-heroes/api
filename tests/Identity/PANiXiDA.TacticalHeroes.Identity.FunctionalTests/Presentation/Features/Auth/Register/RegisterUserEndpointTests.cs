using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Register;

public sealed class RegisterUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST auth register should create an unconfirmed user in PostgreSQL")]
    public async Task PostRegister_Should_PersistUnconfirmedUser_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest(
                " REGISTERED@Example.COM ",
                " registered-hero ",
                "StrongPassword1!"),
            JsonOptions,
            cancellationToken);
        var responseBody = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        responseBody.ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull();
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            responseBody.Id,
            cancellationToken);
        user.ShouldNotBeNull();
        user.Email.ShouldBe("registered@example.com");
        user.UserName.ShouldBe("registered-hero");
        user.EmailConfirmed.ShouldBeFalse();
    }
}
