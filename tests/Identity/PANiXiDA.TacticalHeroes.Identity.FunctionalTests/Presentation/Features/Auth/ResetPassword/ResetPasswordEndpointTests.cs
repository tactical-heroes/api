using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
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

    [Fact(DisplayName = "POST auth reset-password should roll back and allow retry when request is canceled after password save")]
    public async Task PostResetPassword_Should_RollBackAndAllowRetry_When_RequestIsCanceledAfterPasswordSave()
    {
        const string CurrentPassword = "StrongPassword1!";
        const string NewPassword = "NewStrongPassword1!";
        var cancellationToken = TestContext.Current.CancellationToken;
        var createdUser = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "canceled-reset@example.test",
            "canceled-reset-hero",
            CurrentPassword,
            cancellationToken);
        string passwordHash;
        string securityStamp;
        string refreshTokenId;

        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
            var user = await userManager.FindByIdAsync(createdUser.Id.ToString());
            user.ShouldNotBeNull();
            passwordHash = user.PasswordHash!;
            securityStamp = user.SecurityStamp!;
            var refreshToken = await tokenManager.CreateAsync(new OpenIddictTokenDescriptor
            {
                Subject = createdUser.Id.ToString(),
                Type = OpenIddictConstants.TokenTypeIdentifiers.RefreshToken,
                Status = OpenIddictConstants.Statuses.Valid
            }, cancellationToken);
            refreshTokenId = (await tokenManager.GetIdAsync(refreshToken, cancellationToken))!;
        }

        using var requestCancellation = new CancellationTokenSource();
        var interceptor = new CancelAfterPasswordSaveInterceptor(createdUser.Id, passwordHash, requestCancellation);
        await using var factory = new FunctionalTestWebApplicationFactory()
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                services.AddSingleton<IInterceptor>(interceptor)));
        using var client = new HttpClient(factory.Server.CreateHandler(context =>
            context.RequestAborted = requestCancellation.Token))
        {
            BaseAddress = new Uri("https://localhost")
        };
        string resetToken;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(createdUser.Id.ToString());
            user.ShouldNotBeNull();
            resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        }

        var request = new ResetPasswordRequest(createdUser.Id, resetToken, NewPassword);

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/reset-password", request, JsonOptions, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        ((int)response.StatusCode).ShouldBe(StatusCodes.Status499ClientClosedRequest, responseBody);
        interceptor.PasswordSavedInTransaction.ShouldBeTrue();
        requestCancellation.IsCancellationRequested.ShouldBeTrue();

        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
            var user = await userManager.FindByIdAsync(createdUser.Id.ToString());
            user.ShouldNotBeNull();
            user.PasswordHash.ShouldBe(passwordHash);
            user.SecurityStamp.ShouldBe(securityStamp);
            (await userManager.CheckPasswordAsync(user, CurrentPassword)).ShouldBeTrue();
            (await userManager.CheckPasswordAsync(user, NewPassword)).ShouldBeFalse();
            var refreshToken = await tokenManager.FindByIdAsync(refreshTokenId, cancellationToken);
            refreshToken.ShouldNotBeNull();
            (await tokenManager.GetStatusAsync(refreshToken, cancellationToken))
                .ShouldBe(OpenIddictConstants.Statuses.Valid);
        }

        using var retryClient = factory.CreateClient();
        using var retryResponse = await retryClient.PostAsJsonAsync(
            "/api/v1/auth/reset-password", request, JsonOptions, cancellationToken);

        retryResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var verificationScope = Fixture.Services.CreateAsyncScope();
        var verificationUserManager = verificationScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var verificationTokenManager = verificationScope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var updatedUser = await verificationUserManager.FindByIdAsync(createdUser.Id.ToString());
        updatedUser.ShouldNotBeNull();
        (await verificationUserManager.CheckPasswordAsync(updatedUser, NewPassword)).ShouldBeTrue();
        var revokedToken = await verificationTokenManager.FindByIdAsync(refreshTokenId, cancellationToken);
        revokedToken.ShouldNotBeNull();
        (await verificationTokenManager.GetStatusAsync(revokedToken, cancellationToken))
            .ShouldBe(OpenIddictConstants.Statuses.Revoked);
    }

    private sealed class CancelAfterPasswordSaveInterceptor(
        Guid userId,
        string originalPasswordHash,
        CancellationTokenSource requestCancellation) : SaveChangesInterceptor
    {
        public bool PasswordSavedInTransaction { get; private set; }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            var user = context?.ChangeTracker.Entries<ApplicationUser>()
                .Select(entry => entry.Entity)
                .SingleOrDefault(user => user.Id == userId);

            if (user is not null && user.PasswordHash != originalPasswordHash)
            {
                PasswordSavedInTransaction = context!.Database.CurrentTransaction is not null;
                requestCancellation.Cancel();
            }

            return ValueTask.FromResult(result);
        }
    }
}
