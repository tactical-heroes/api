using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

using OpenIddict.Validation.SystemNetHttp;

using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

internal sealed class FunctionalTestWebApplicationFactory(string environmentName = "Test")
    : WebApplicationFactory<Program>
{
    private int _introspectionRequestCount;

    public CapturingEventBus EventBus { get; } = new();

    public int IntrospectionRequestCount => Volatile.Read(ref _introspectionRequestCount);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["OpenIddictValidationOptions:ClientSecret"] = "functional-test-introspection-secret",
                ["Identity:Provider:Clients:1:ClientSecret"] = "functional-test-introspection-secret"
            }));
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.ConfigureAll<HttpClientFactoryOptions>(options =>
                options.HttpMessageHandlerBuilderActions.Add(handler =>
                {
                    if (handler.Name?.StartsWith("OpenIddict.Validation.SystemNetHttp", StringComparison.Ordinal) == true)
                    {
                        handler.PrimaryHandler = new IntrospectionTrackingHandler(this);
                    }
                }));
            services.Configure<OpenIddictValidationSystemNetHttpOptions>(options =>
                options.HttpResiliencePipeline = null);

            var seederHostedService = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IHostedService) &&
                descriptor.ImplementationType == typeof(IdentityProviderApplicationSeederHostedService));

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

    private sealed class IntrospectionTrackingHandler(FunctionalTestWebApplicationFactory factory) : HttpClientHandler
    {
        private readonly HttpMessageInvoker _transport = new(factory.Server.CreateHandler());

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
