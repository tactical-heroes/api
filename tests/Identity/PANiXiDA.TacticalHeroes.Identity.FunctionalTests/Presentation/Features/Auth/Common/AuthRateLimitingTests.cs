using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common.Options.AuthRateLimit;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Common;

public sealed class AuthRateLimitingTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Auth endpoints should return 429 and Retry-After when ip limit is exceeded")]
    [InlineData("login")]
    [InlineData("register")]
    [InlineData("forgot-password")]
    [InlineData("reset-password")]
    [InlineData("resend-confirmation-email")]
    [InlineData("confirm-email")]
    public async Task PostAuth_Should_Return429AndRetryAfter_When_IpLimitIsExceeded(string endpoint)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.PostConfigure<AuthRateLimitOptions>(options => SetLimits(options, TimeSpan.FromMinutes(1)))));
        using var client = CreateClient(factory, "192.0.2.1");
        var path = $"/api/v1/auth/{endpoint}";

        using var first = await client.PostAsJsonAsync(path, new { }, cancellationToken);
        using var second = await client.PostAsJsonAsync(path, new { }, cancellationToken);

        first.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        second.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        second.Headers.RetryAfter.ShouldNotBeNull();
        second.Headers.RetryAfter.Delta.GetValueOrDefault().ShouldBeGreaterThan(TimeSpan.Zero);
        using var otherIp = CreateClient(factory, "192.0.2.2");
        using var independent = await otherIp.PostAsJsonAsync(path, new { }, cancellationToken);
        independent.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Auth limits should allow another request when window ends")]
    public async Task PostAuth_Should_AllowAnotherRequest_When_WindowEnds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.PostConfigure<AuthRateLimitOptions>(options => SetLimits(options, TimeSpan.FromSeconds(1)))));
        using var client = CreateClient(factory, "192.0.2.1");
        const string Path = "/api/v1/auth/forgot-password";
        using var first = await client.PostAsJsonAsync(Path, new { }, cancellationToken);
        using var rejected = await client.PostAsJsonAsync(Path, new { }, cancellationToken);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        await Task.Delay(TimeSpan.FromMilliseconds(1250), cancellationToken);

        using var retry = await client.PostAsJsonAsync(Path, new { }, cancellationToken);
        first.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        retry.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "Forwarded IPs should affect partitions only when forwarded ip changes")]
    [InlineData("127.0.0.1", HttpStatusCode.BadRequest)]
    [InlineData("192.0.2.10", HttpStatusCode.TooManyRequests)]
    public async Task PostAuth_Should_RespectProxyTrust_When_ForwardedIpChanges(string proxy, HttpStatusCode expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.PostConfigure<AuthRateLimitOptions>(options => SetLimits(options, TimeSpan.FromMinutes(1)))));
        using var client = CreateClient(factory, proxy);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.1");
        using var first = await client.PostAsJsonAsync("/api/v1/auth/login", new { }, cancellationToken);
        client.DefaultRequestHeaders.Remove("X-Forwarded-For");
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.2");

        using var second = await client.PostAsJsonAsync("/api/v1/auth/login", new { }, cancellationToken);

        first.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        second.StatusCode.ShouldBe(expected);
        using var independentPolicy = await client.PostAsJsonAsync("/api/v1/auth/register", new { }, cancellationToken);
        independentPolicy.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Password change should share a limit across sessions and IPs when subject is same")]
    public async Task PostChangePassword_Should_ShareLimitAcrossSessionsAndIps_When_SubjectIsSame()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "limited@example.test", "limited-user", "StrongPassword1!", cancellationToken);
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "other@example.test", "other-user", "StrongPassword1!", cancellationToken);
        using var login = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            login, "limited@example.test", "StrongPassword1!", cancellationToken);
        using var otherLogin = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            otherLogin, "other@example.test", "StrongPassword1!", cancellationToken);
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.PostConfigure<AuthRateLimitOptions>(options => SetLimits(options, TimeSpan.FromMinutes(1)))));
        using var firstIp = CreateClient(factory, "192.0.2.1");
        using var secondIp = CreateClient(factory, "192.0.2.2");
        firstIp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        secondIp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var request = new ChangePasswordRequest("WrongPassword1!", "NewStrongPassword1!");
        using var first = await firstIp.PostAsJsonAsync(
            "/api/v1/auth/change-password",
            request,
            JsonOptions,
            cancellationToken);

        using var second = await secondIp.PostAsJsonAsync(
            "/api/v1/auth/change-password",
            request,
            JsonOptions,
            cancellationToken);

        first.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        second.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        secondIp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherTokens.AccessToken);
        using var independent = await secondIp.PostAsJsonAsync(
            "/api/v1/auth/change-password",
            request,
            JsonOptions,
            cancellationToken);
        independent.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static HttpClient CreateClient(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory,
        string ip)
    {
        return new HttpClient(factory.Server.CreateHandler(context =>
            context.Connection.RemoteIpAddress = IPAddress.Parse(ip)))
        {
            BaseAddress = new Uri("https://localhost")
        };
    }

    private static void SetLimits(AuthRateLimitOptions options, TimeSpan window)
    {
        foreach (var policy in new[]
        {
            options.Login,
            options.Register,
            options.ForgotPassword,
            options.ResetPassword,
            options.ResendConfirmationEmail,
            options.ConfirmEmail,
            options.ChangePassword
        })
        {
            policy.PermitLimit = 1;
            policy.Window = window;
        }
    }
}
