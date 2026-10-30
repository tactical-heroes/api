using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

using OpenIddict.Validation.SystemNetHttp;

using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.ResultPattern;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Auth;
using PANiXiDA.TacticalHeroes.Testing.Databases;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

internal sealed class FunctionalTestWebApplicationFactory(string connectionString, string environmentName = "Test")
    : WebApplicationFactory<Program>
{
    private static readonly Lock s_hostConfigurationLock = new();

    private int _introspectionRequestCount;

    public CapturingEventBus EventBus { get; } = new();

    public int IntrospectionRequestCount => Volatile.Read(ref _introspectionRequestCount);

    internal static ValueTask InitializeDatabaseAsync(
        PostgreSqlTestDatabase database,
        CancellationToken cancellationToken)
    {
        lock (s_hostConfigurationLock)
        {
            return database.InitializeAsync(cancellationToken);
        }
    }

    internal WebApplicationFactory<Program> WithAuthenticatedUserReadPause(
        TaskCompletionSource reached,
        TaskCompletionSource resume,
        bool readBeforePause,
        Func<bool> shouldPause)
    {
        return WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.AddScoped<AuthenticatedUserService>();
            services.AddScoped<IAuthenticatedUserService>(provider => new PausingAuthenticatedUserService(
                provider.GetRequiredService<AuthenticatedUserService>(),
                reached,
                resume,
                readBeforePause,
                shouldPause));
        }));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        lock (s_hostConfigurationLock)
        {
            var variable = PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable;
            var previous = Environment.GetEnvironmentVariable(variable);
            const string MatchmakingVariable = "Matchmaking__Schedules__ProcessMatchmakingCommand__Enabled";
            var previousMatchmaking = Environment.GetEnvironmentVariable(MatchmakingVariable);
            const string ExpirationVariable = "Matchmaking__Schedules__ExpireMatchSearchesCommand__Enabled";
            var previousExpiration = Environment.GetEnvironmentVariable(ExpirationVariable);
            const string FileManagerScheduleVariable = "FileManager__Schedules__ReconcileFileUploadsCommand__Enabled";
            var previousFileManagerSchedule = Environment.GetEnvironmentVariable(FileManagerScheduleVariable);
            try
            {
                Environment.SetEnvironmentVariable(variable, connectionString);
                Environment.SetEnvironmentVariable(MatchmakingVariable, "false");
                Environment.SetEnvironmentVariable(ExpirationVariable, "false");
                Environment.SetEnvironmentVariable(FileManagerScheduleVariable, "false");
                return base.CreateHost(builder);
            }
            finally
            {
                Environment.SetEnvironmentVariable(variable, previous);
                Environment.SetEnvironmentVariable(MatchmakingVariable, previousMatchmaking);
                Environment.SetEnvironmentVariable(ExpirationVariable, previousExpiration);
                Environment.SetEnvironmentVariable(FileManagerScheduleVariable, previousFileManagerSchedule);
            }
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSqlConnectionString"] = connectionString,
                ["OpenIddictValidationOptions:ClientSecret"] = "functional-test-introspection-secret",
                ["Identity:Provider:Clients:1:ClientSecret"] = "functional-test-introspection-secret"
            }));
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.Configure<PasswordHasherOptions>(options => options.IterationCount = 1_000);
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

            services.RemoveAll<IEventBus>();
            services.AddSingleton(EventBus);
            services.AddSingleton<IEventBus>(serviceProvider =>
                serviceProvider.GetRequiredService<CapturingEventBus>());
            services.RunWolverineInSoloMode();
        });
    }

    private sealed class PausingAuthenticatedUserService(
        AuthenticatedUserService inner,
        TaskCompletionSource reached,
        TaskCompletionSource resume,
        bool readBeforePause,
        Func<bool> shouldPause) : IAuthenticatedUserService
    {
        public async Task<Result<AuthenticatedUserReadModel>> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (!shouldPause())
            {
                return await inner.GetByIdAsync(userId, cancellationToken);
            }

            if (readBeforePause)
            {
                var user = await inner.GetByIdAsync(userId, cancellationToken);
                await PauseAsync(cancellationToken);
                return user;
            }

            await PauseAsync(cancellationToken);
            return await inner.GetByIdAsync(userId, cancellationToken);
        }

        private async Task PauseAsync(CancellationToken cancellationToken)
        {
            reached.TrySetResult();
            await resume.Task.WaitAsync(cancellationToken);
        }
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
