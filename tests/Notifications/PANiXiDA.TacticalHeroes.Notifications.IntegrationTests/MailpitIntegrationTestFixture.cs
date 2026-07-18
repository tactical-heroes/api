using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Email.Options;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Notifications.IntegrationTests;

public sealed class MailpitIntegrationTestFixture : IAsyncLifetime
{
    private const ushort MailpitHttpPort = 8025;
    private const ushort MailpitSmtpPort = 1025;

    private readonly IContainer _mailpitContainer = new ContainerBuilder("axllent/mailpit:v1.30.4")
        .WithEnvironment("MP_SMTP_DISABLE_RDNS", "true")
        .WithPortBinding(MailpitHttpPort, assignRandomHostPort: true)
        .WithPortBinding(MailpitSmtpPort, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request
                .ForPort(MailpitHttpPort)
                .ForPath("/readyz")))
        .Build();

    private HttpClient _mailpitClient = null!;
    private IHost _host = null!;

    public IMessageBus MessageBus => _host.Services.GetRequiredService<IMessageBus>();

    public async ValueTask InitializeAsync()
    {
        await _mailpitContainer.StartAsync(TestContext.Current.CancellationToken);

        _mailpitClient = new HttpClient
        {
            BaseAddress = new UriBuilder(
                Uri.UriSchemeHttp,
                _mailpitContainer.Hostname,
                _mailpitContainer.GetMappedPublicPort(MailpitHttpPort)).Uri
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{SmtpOptions.SectionName}:Host"] = _mailpitContainer.Hostname,
                [$"{SmtpOptions.SectionName}:Port"] =
                    _mailpitContainer.GetMappedPublicPort(MailpitSmtpPort).ToString(),
                [$"{SmtpOptions.SectionName}:SocketOptions"] = "None",
                [$"{SmtpOptions.SectionName}:SenderEmail"] =
                    "no-reply@tactical-heroes.local",
                [$"{SmtpOptions.SectionName}:SenderName"] = "Tactical Heroes"
            })
            .Build();

        var hostBuilder = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services => services.AddInfrastructure(configuration))
            .UseWolverine();
        hostBuilder.UseInfrastructure(configuration);

        _host = hostBuilder.Build();
        await _host.StartAsync(TestContext.Current.CancellationToken);
    }

    public async Task WaitForMessageAsync(
        string subject,
        string recipientEmail,
        CancellationToken cancellationToken)
    {
        var lastMessages = string.Empty;

        for (var attempt = 0; attempt < 50; attempt++)
        {
            lastMessages = await _mailpitClient.GetStringAsync(
                "api/v1/messages",
                cancellationToken);

            if (lastMessages.Contains(subject, StringComparison.Ordinal) &&
                lastMessages.Contains(recipientEmail, StringComparison.Ordinal))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        throw new TimeoutException(
            $"Mailpit did not receive email with subject '{subject}'. Last response: {lastMessages}");
    }

    public async Task<string> WaitForBodyAsync(
        string format,
        string expectedContent,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            using var response = await _mailpitClient.GetAsync(
                $"view/latest.{format}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (body.Contains(expectedContent, StringComparison.Ordinal))
                {
                    return body;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        throw new TimeoutException(
            $"Mailpit did not render the latest {format} body containing '{expectedContent}'.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        _mailpitClient?.Dispose();
        await _mailpitContainer.DisposeAsync();
    }
}
