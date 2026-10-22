using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

using OpenIddict.Validation.SystemNetHttp;

using Npgsql;

using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common.Options.AuthRateLimit;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

internal sealed class FunctionalTestWebApplicationFactory(string environmentName = "Test", bool captureEvents = true)
    : WebApplicationFactory<Program>
{
    private int _introspectionRequestCount;

    public CapturingEventBus EventBus { get; } = new();

    public int IntrospectionRequestCount => Volatile.Read(ref _introspectionRequestCount);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var connectionString = new NpgsqlConnectionStringBuilder(
                configuration.Build().GetConnectionString("PostgreSqlConnectionString"))
            {
                MaxPoolSize = 8,
                ConnectionIdleLifetime = 1,
                ConnectionPruningInterval = 1
            };
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSqlConnectionString"] = connectionString.ConnectionString,
                ["OpenIddictValidationOptions:ClientSecret"] = "functional-test-introspection-secret",
                ["Identity:Provider:Clients:1:ClientSecret"] = "functional-test-introspection-secret"
            });
        });
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.PostConfigure<AuthRateLimitOptions>(options =>
            {
                options.Login.PermitLimit = int.MaxValue;
                options.Register.PermitLimit = int.MaxValue;
                options.ForgotPassword.PermitLimit = int.MaxValue;
                options.ResetPassword.PermitLimit = int.MaxValue;
                options.ResendConfirmationEmail.PermitLimit = int.MaxValue;
                options.ConfirmEmail.PermitLimit = int.MaxValue;
                options.ChangePassword.PermitLimit = int.MaxValue;
            });
            services.AddScoped<IOAuthClientsRepository, PermissionTestClientsRepository>();
            services.ConfigureAll<HttpClientFactoryOptions>(options =>
                options.HttpMessageHandlerBuilderActions.Add(handler =>
                {
                    if (handler.Name?
                        .StartsWith("OpenIddict.Validation.SystemNetHttp", StringComparison.Ordinal) == true)
                    {
                        handler.PrimaryHandler = new IntrospectionTrackingHandler(this);
                    }
                }));
            services.Configure<OpenIddictValidationSystemNetHttpOptions>(options =>
                options.HttpResiliencePipeline = null);

            var seederHostedService = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                    && descriptor.ImplementationType == typeof(IdentityProviderApplicationSeederHostedService));

            if (seederHostedService is not null)
            {
                services.Remove(seederHostedService);
            }

            if (captureEvents)
            {
                services.RemoveAll<IEventBus>();
                services.AddSingleton(EventBus);
                services.AddSingleton<IEventBus>(serviceProvider =>
                    serviceProvider.GetRequiredService<CapturingEventBus>());
            }
            services.RunWolverineInSoloMode();
        });
    }

    private sealed class IntrospectionTrackingHandler(FunctionalTestWebApplicationFactory factory) : HttpClientHandler
    {
        private readonly HttpMessageInvoker _transport = new(factory.Server.CreateHandler());

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath == "/connect/introspect")
            {
                Interlocked.Increment(ref factory._introspectionRequestCount);
            }

            return _transport.SendAsync(request, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _transport.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
