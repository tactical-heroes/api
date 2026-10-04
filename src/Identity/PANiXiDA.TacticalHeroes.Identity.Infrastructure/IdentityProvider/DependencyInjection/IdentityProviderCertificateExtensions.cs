using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Common;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Certificates;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.IdentityProvider;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.DependencyInjection;

internal static class IdentityProviderCertificateExtensions
{
    public static void AddIdentityProviderCertificates(
        this OpenIddictServerBuilder builder,
        IdentityProviderOptions options,
        IHostEnvironment? environment)
    {
        builder.Services.AddOptions<OpenIddictServerOptions>().ValidateOnStart();

        if (options.SigningCertificates.Count == 0 && options.EncryptionCertificates.Count == 0 &&
            (environment is null || environment.IsDevelopment() || environment.IsEnvironment(EnvironmentConstants.Test)))
        {
            builder.AddDevelopmentEncryptionCertificate();
            builder.AddDevelopmentSigningCertificate();
            return;
        }

        builder.Configure(_ =>
        {
            if (options.SigningCertificates.Count == 0 || options.EncryptionCertificates.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{IdentityProviderOptions.SectionName}:SigningCertificates and " +
                    $"{IdentityProviderOptions.SectionName}:EncryptionCertificates must both be configured.");
            }
        });

        for (var index = 0; index < options.SigningCertificates.Count; index++)
        {
            builder.AddSigningCertificate(LoadCertificate(
                options.SigningCertificates[index],
                $"{IdentityProviderOptions.SectionName}:SigningCertificates:{index}"));
        }

        for (var index = 0; index < options.EncryptionCertificates.Count; index++)
        {
            builder.AddEncryptionCertificate(LoadCertificate(
                options.EncryptionCertificates[index],
                $"{IdentityProviderOptions.SectionName}:EncryptionCertificates:{index}"));
        }
    }

    private static X509Certificate2 LoadCertificate(IdentityProviderCertificateOptions options, string path)
    {
        try
        {
            return X509CertificateLoader.LoadPkcs12(
                data: Convert.FromBase64String(options.PfxBase64),
                password: options.Password,
                keyStorageFlags: X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            throw new InvalidOperationException($"{path} must contain a valid PFX and its password.", exception);
        }
    }
}
