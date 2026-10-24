using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.ResultPattern;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Login;

public sealed class LoginEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Theory(DisplayName = "POST auth login should create no session when authentication model is unavailable")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PostLogin_Should_CreateNoSession_When_AuthenticationModelIsUnavailable(bool validPassword)
    {
        var ct = TestContext.Current.CancellationToken;
        var user = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "unavailable-auth@example.test",
            userName: "unavailable-auth",
            password: Password,
            claims: null,
            cancellationToken: ct);
        var service = new UnavailableAuthenticatedUserService();
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAuthenticatedUserService>();
            services.AddSingleton<IAuthenticatedUserService>(service);
        }));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        var path = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: ct);

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(
                "unavailable-auth@example.test",
                validPassword ? Password : "WrongPassword1!",
                $"https://localhost{path}"),
            JsonOptions,
            ct);

        response.StatusCode.ShouldBe(validPassword ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized);
        response.Headers.Contains("Set-Cookie").ShouldBeFalse();
        service.UserId.ShouldBe(validPassword ? user.Id : null);
    }

    [Fact(DisplayName = "POST auth login should persist lockout when invalid attempts reach the limit")]
    public async Task PostLogin_Should_PersistLockout_When_InvalidAttemptsReachTheLimit()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "lockout@example.test",
            userName: "lockout-hero",
            password: Password,
            claims: null,
            cancellationToken: cancellationToken);
        int attemptLimit;
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            attemptLimit = manager.Options.Lockout.MaxFailedAccessAttempts;
        }
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: cancellationToken);

        for (var attempt = 0; attempt < attemptLimit; attempt++)
        {
            using var response = await client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new LoginRequest("lockout@example.test", "WrongPassword1!", $"https://localhost{authorizePath}"),
                JsonOptions,
                cancellationToken);
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        await using var verification = Fixture.Services.CreateAsyncScope();
        var verificationManager = verification.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await verificationManager.FindByIdAsync(created.Id.ToString());
        user.ShouldNotBeNull();
        (await verificationManager.IsLockedOutAsync(user)).ShouldBeTrue();

        using var lockedOutResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("lockout@example.test", Password, $"https://localhost{authorizePath}"),
            JsonOptions,
            cancellationToken);
        lockedOutResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "POST auth login should update the hash and reset failures when password needs rehashing")]
    public async Task PostLogin_Should_UpdateHashAndResetFailures_When_PasswordNeedsRehashing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "reset-attempts@example.test",
            userName: "reset-attempts-hero",
            password: Password,
            claims: null,
            cancellationToken: cancellationToken);
        string originalHash;
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await manager.FindByIdAsync(created.Id.ToString());
            user.ShouldNotBeNull();
            var hasher = new PasswordHasher<ApplicationUser>(
                Options.Create(new PasswordHasherOptions { IterationCount = 1 }));
            originalHash = hasher.HashPassword(user, Password);
            user.PasswordHash = originalHash;
            (await manager.UpdateAsync(user)).Succeeded.ShouldBeTrue();
        }
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: cancellationToken);
        using var failedResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("reset-attempts@example.test", "WrongPassword1!", $"https://localhost{authorizePath}"),
            JsonOptions,
            cancellationToken);
        failedResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await manager.FindByIdAsync(created.Id.ToString());
            user.ShouldNotBeNull();
            user.AccessFailedCount.ShouldBe(1);
        }

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("reset-attempts@example.test", Password, $"https://localhost{authorizePath}"),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        await using var verification = Fixture.Services.CreateAsyncScope();
        var verificationManager = verification.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var verifiedUser = await verificationManager.FindByIdAsync(created.Id.ToString());
        verifiedUser.ShouldNotBeNull();
        verifiedUser.AccessFailedCount.ShouldBe(0);
        verifiedUser.PasswordHash.ShouldNotBe(originalHash);
    }

    [Fact(
        DisplayName = "POST auth login should authenticate a confirmed user from PostgreSQL when credentials are valid")]
    public async Task PostLogin_Should_CreateAuthenticationSession_When_CredentialsAreValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "login@example.test",
            userName: "login-hero",
            password: Password,
            claims: null,
            cancellationToken: cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: cancellationToken);

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(
                "login@example.test",
                Password,
                $"https://localhost{authorizePath}"),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldBe(new Uri($"https://localhost{authorizePath}"));
        response.Headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue();
        cookies.ShouldContain(cookie =>
            cookie.Contains(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "POST auth login should reject an invalid password when password is invalid")]
    public async Task PostLogin_Should_ReturnUnauthorized_When_PasswordIsInvalid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "invalid-login@example.test",
            userName: "invalid-login-hero",
            password: Password,
            claims: null,
            cancellationToken: cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: cancellationToken);

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(
                "invalid-login@example.test",
                "WrongPassword1!",
                $"https://localhost{authorizePath}"),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private sealed class UnavailableAuthenticatedUserService : IAuthenticatedUserService
    {
        public Guid? UserId { get; private set; }

        public Task<Result<AuthenticatedUserReadModel>> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            UserId = userId;
            return Task.FromResult(
                Result.Failure<AuthenticatedUserReadModel>(Error.Forbidden(message: "User is blocked.")));
        }
    }
}
