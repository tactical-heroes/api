using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Certificates;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.IdentityProvider;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider;

public sealed class IdentityProviderCertificatesOptionsValidatorTests
{
    [Theory(DisplayName = "Validate should reject null collections when a certificate collection is null")]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_Should_RejectNullCollections_When_ACertificateCollectionIsNull(bool signing)
    {
        var validator = new IdentityProviderCertificatesOptionsValidator();
        var options = new IdentityProviderOptions
        {
            SigningCertificates = signing ? null! : [new() { PfxBase64 = "configured" }],
            EncryptionCertificates = signing ? [new() { PfxBase64 = "configured" }] : null!
        };

        var result = validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldHaveSingleItem().ShouldContain(signing ? "SigningCertificates" : "EncryptionCertificates");
    }

    [Fact(DisplayName = "Validate should report indexed failures when certificate collections contain null entries")]
    public void Validate_Should_ReportIndexedFailures_When_CertificateCollectionsContainNullEntries()
    {
        var validator = new IdentityProviderCertificatesOptionsValidator();
        var options = new IdentityProviderOptions
        {
            SigningCertificates = [new() { PfxBase64 = "configured" }, null!],
            EncryptionCertificates = [null!]
        };

        var result = validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe([
            "Identity:Provider:SigningCertificates:1 must be configured.",
            "Identity:Provider:EncryptionCertificates:0 must be configured."
        ]);
    }

    [Theory(DisplayName = "Validate should report indexed failures when certificate data is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_Should_ReportIndexedFailures_When_CertificateDataIsEmpty(string? pfxBase64)
    {
        var validator = new IdentityProviderCertificatesOptionsValidator();
        var options = new IdentityProviderOptions
        {
            SigningCertificates = [new() { PfxBase64 = pfxBase64! }],
            EncryptionCertificates = [new() { PfxBase64 = pfxBase64! }]
        };

        var result = validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe([
            "Identity:Provider:SigningCertificates:0:PfxBase64 must not be empty.",
            "Identity:Provider:EncryptionCertificates:0:PfxBase64 must not be empty."
        ]);
    }

    [Fact(DisplayName = "Validate should accept optional passwords when both certificate collections are configured")]
    public void Validate_Should_AcceptOptionalPasswords_When_BothCertificateCollectionsAreConfigured()
    {
        var validator = new IdentityProviderCertificatesOptionsValidator();
        var options = new IdentityProviderOptions
        {
            SigningCertificates = [new() { PfxBase64 = "configured" }],
            EncryptionCertificates = [new() { PfxBase64 = "configured", Password = string.Empty }]
        };

        var result = validator.Validate(null, options);

        result.Succeeded.ShouldBeTrue();
    }

    [Theory(DisplayName = "AddIdentityProviderOptionsValidators should apply environment rules when options are resolved")]
    [InlineData("Production", false)]
    [InlineData("Staging", false)]
    [InlineData("Development", true)]
    [InlineData("Test", true)]
    public void AddIdentityProviderOptionsValidators_Should_ApplyEnvironmentRules_When_OptionsAreResolved(
        string environment,
        bool allowsMissingCertificates)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = environment
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:Provider:Issuer"] = "https://localhost/",
            ["Identity:Provider:Audience"] = "tactical-heroes-api",
            ["Identity:Provider:Clients:0:ClientId"] = "certificate-tests",
            ["Identity:Provider:Clients:0:DisplayName"] = "Certificate Tests",
            ["Identity:Provider:Clients:0:ClientType"] = OpenIddictConstants.ClientTypes.Public,
            ["Identity:Provider:Clients:0:GrantTypes:0"] = OpenIddictConstants.GrantTypes.AuthorizationCode
        });
        builder.Services.AddIdentityProviderOptionsValidators();
        builder.Services.AddOptions<IdentityProviderOptions>()
            .Bind(builder.Configuration.GetSection(IdentityProviderOptions.SectionName));
        using var provider = builder.Services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<IdentityProviderOptions>>();

        if (allowsMissingCertificates)
        {
            options.Value.SigningCertificates.ShouldBeEmpty();
            options.Value.EncryptionCertificates.ShouldBeEmpty();
        }
        else
        {
            var exception = Should.Throw<OptionsValidationException>(() => options.Value);
            exception.Failures.ShouldBe([
                "Identity:Provider:SigningCertificates must contain at least one certificate.",
                "Identity:Provider:EncryptionCertificates must contain at least one certificate."
            ]);
        }
    }
}
