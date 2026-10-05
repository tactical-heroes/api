using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Common;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.IdentityProvider;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Certificates;

internal sealed class IdentityProviderCertificatesOptionsValidator(IHostEnvironment? environment = null)
    : IValidateOptions<IdentityProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, IdentityProviderOptions options)
    {
        if (options.SigningCertificates is { Count: 0 } && options.EncryptionCertificates is { Count: 0 } &&
            (environment is null || environment.IsDevelopment() || environment.IsEnvironment(environmentName: EnvironmentConstants.Test)))
        {
            return ValidateOptionsResult.Success;
        }

        List<string> failures = [];

        ValidateCertificates(
            certificates: options.SigningCertificates,
            path: $"{IdentityProviderOptions.SectionName}:SigningCertificates",
            failures: failures);
        ValidateCertificates(
            certificates: options.EncryptionCertificates,
            path: $"{IdentityProviderOptions.SectionName}:EncryptionCertificates",
            failures: failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }

    private static void ValidateCertificates(
        List<IdentityProviderCertificateOptions>? certificates,
        string path,
        List<string> failures)
    {
        if (certificates is null || certificates.Count == 0)
        {
            failures.Add(item: $"{path} must contain at least one certificate.");
            return;
        }

        for (var index = 0; index < certificates.Count; index++)
        {
            var certificate = certificates[index];

            if (certificate is null)
            {
                failures.Add(item: $"{path}:{index} must be configured.");
            }
            else if (string.IsNullOrWhiteSpace(certificate.PfxBase64))
            {
                failures.Add(item: $"{path}:{index}:PfxBase64 must not be empty.");
            }
        }
    }
}
