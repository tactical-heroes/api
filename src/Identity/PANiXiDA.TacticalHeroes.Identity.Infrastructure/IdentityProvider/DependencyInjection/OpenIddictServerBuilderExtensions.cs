using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Certificates;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.IdentityProvider;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.DependencyInjection;

internal static class OpenIddictServerBuilderExtensions
{
    public static void AddIdentityProviderCertificates(
        this OpenIddictServerBuilder builder,
        IdentityProviderOptions options,
        IHostEnvironment? environment)
    {
        builder.Services.AddOptions<OpenIddictServerOptions>().ValidateOnStart();

        var validation = new IdentityProviderCertificatesOptionsValidator(environment).Validate(name: null, options: options);

        if (validation.Failed)
        {
            builder.Configure(_ => throw new OptionsValidationException(
                optionsName: Microsoft.Extensions.Options.Options.DefaultName,
                optionsType: typeof(IdentityProviderOptions),
                failureMessages: validation.Failures));
            return;
        }

        if (options.SigningCertificates.Count == 0 && options.EncryptionCertificates.Count == 0)
        {
            builder.AddDevelopmentEncryptionCertificate();
            builder.AddDevelopmentSigningCertificate();
            return;
        }

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
