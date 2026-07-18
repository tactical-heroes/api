using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ForgotPassword;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string CurrentPassword = "StrongPassword1!";
    private const string NewPassword = "NewStrongPassword1!";

    [Fact(DisplayName = "Password reset should persist the new password obtained through the API flow")]
    public async Task PasswordReset_Should_ReplacePassword_When_IssuedTokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createdUser = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "forgot@example.test",
            "forgot-hero",
            CurrentPassword,
            cancellationToken);
        Fixture.EventBus.Clear();

        using var forgotResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new ForgotPasswordRequest("forgot@example.test"),
            JsonOptions,
            cancellationToken);

        forgotResponse.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        var passwordReset = Fixture.EventBus.Single<PasswordResetRequested>();

        using var resetResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new ResetPasswordRequest(
                createdUser.Id,
                passwordReset.PasswordResetToken,
                NewPassword),
            JsonOptions,
            cancellationToken);

        resetResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var scope = Fixture.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(createdUser.Id.ToString());

        user.ShouldNotBeNull();
        (await userManager.CheckPasswordAsync(user, CurrentPassword)).ShouldBeFalse();
        (await userManager.CheckPasswordAsync(user, NewPassword)).ShouldBeTrue();
    }
}
