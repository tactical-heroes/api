using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ResetPassword;

public sealed class ResetPasswordEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST auth reset-password should return not found for a missing user when user does not exist")]
    public async Task PostResetPassword_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new ResetPasswordRequest(
                Guid.CreateVersion7(),
                "password-reset-token",
                "NewStrongPassword1!"),
            JsonOptions,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
