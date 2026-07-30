using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;
using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Email.Options;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Email.DependencyInjection;

internal static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddSingleton<
            IValidateOptions<SmtpOptions>,
            SmtpOptionsValidator>();
        serviceCollection
            .AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .ValidateOnStart();

        serviceCollection.AddTransient<IEmailSender, MailKitEmailSender>();

        return serviceCollection;
    }
}
