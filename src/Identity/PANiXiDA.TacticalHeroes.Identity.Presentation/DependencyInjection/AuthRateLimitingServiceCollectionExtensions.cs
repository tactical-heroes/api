using System.Globalization;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common.Options.AuthRateLimit;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

internal static class AuthRateLimitingServiceCollectionExtensions
{
    internal static IServiceCollection AddAuthRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<AuthRateLimitOptions>, AuthRateLimitOptionsValidator>();
        services.AddOptions<AuthRateLimitOptions>()
            .Bind(configuration.GetSection(AuthRateLimitOptions.SectionName))
            .ValidateOnStart();
        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>().Configure<IOptions<AuthRateLimitOptions>>((limiter, options) =>
        {
            var policies = options.Value;
            AddPolicy(
                limiter: limiter,
                name: AuthRateLimitPolicies.Login,
                policy: policies.Login);
            AddPolicy(
                limiter: limiter,
                name: AuthRateLimitPolicies.Register,
                policy: policies.Register);
            AddPolicy(
                limiter: limiter,
                name: AuthRateLimitPolicies.ForgotPassword,
                policy: policies.ForgotPassword);
            AddPolicy(
                limiter: limiter,
                name: AuthRateLimitPolicies.ResetPassword,
                policy: policies.ResetPassword);
            AddPolicy(
                limiter: limiter,
                name: AuthRateLimitPolicies.ResendConfirmationEmail,
                policy: policies.ResendConfirmationEmail);
            AddPolicy(
                limiter: limiter,
                name: AuthRateLimitPolicies.ConfirmEmail,
                policy: policies.ConfirmEmail);
            AddPolicy(
                limiter: limiter,
                name: AuthRateLimitPolicies.ChangePassword,
                policy: policies.ChangePassword,
                perUser: true);
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = async (context, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Max(1, Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                }

                await TypedResults.Problem(
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Too many requests.")
                    .ExecuteAsync(context.HttpContext);
            };
        });

        return services;
    }

    private static void AddPolicy(
        RateLimiterOptions limiter,
        string name,
        AuthEndpointRateLimitOptions policy,
        bool perUser = false)
    {
        limiter.AddPolicy(
            name,
            context =>
            {
                var address = context.Connection.RemoteIpAddress;
                var ip = address?.IsIPv4MappedToIPv6 == true ? address.MapToIPv4().ToString() : address?.ToString();
                var key = perUser ? context.User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value : ip;

                return RateLimitPartition.GetFixedWindowLimiter(
                    key ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = policy.PermitLimit,
                        Window = policy.Window,
                        QueueLimit = 0
                    });
            });
    }
}
