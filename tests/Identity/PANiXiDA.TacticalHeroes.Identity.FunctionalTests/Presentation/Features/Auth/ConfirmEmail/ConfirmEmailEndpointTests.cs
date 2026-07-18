using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ConfirmEmail;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST auth confirm-email should confirm the persisted user with the issued token")]
    public async Task PostConfirmEmail_Should_ConfirmUser_When_TokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var registerResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest(
                "confirm@example.test",
                "confirm-hero",
                "StrongPassword1!"),
            JsonOptions,
            cancellationToken);
        var createdUser = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>(
            JsonOptions,
            cancellationToken);

        registerResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createdUser.ShouldNotBeNull();
        var confirmation = Fixture.EventBus.Single<EmailConfirmationRequested>();

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/confirm-email",
            new ConfirmEmailRequest(createdUser.Id, confirmation.ConfirmationToken),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var user = await UserDatabaseTestHelper.FindAsync(Fixture, createdUser.Id, cancellationToken);
        user.ShouldNotBeNull();
        user.EmailConfirmed.ShouldBeTrue();
    }
}
