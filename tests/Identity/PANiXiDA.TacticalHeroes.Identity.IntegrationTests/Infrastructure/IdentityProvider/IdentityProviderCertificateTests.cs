using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider;

public sealed class IdentityProviderCertificateTests
{
    private const string CertificatePassword = "test-certificate-password";
    private const string SigningPrefix = "Identity:Provider:SigningCertificates:0";
    private const string EncryptionPrefix = "Identity:Provider:EncryptionCertificates:0";

    [Theory(DisplayName = "AddIdentityProvider should use configured certificates when certificates are provided")]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Test")]
    public void AddIdentityProvider_Should_UseConfiguredCertificates_When_CertificatesAreProvided(string environment)
    {
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        var configuration = CreateConfiguration(signing, encryption);

        var options = CreateServerOptions(configuration, environment);

        var signingKey = options.SigningCredentials.ShouldHaveSingleItem().Key.ShouldBeOfType<X509SecurityKey>();
        var encryptionKey = options.EncryptionCredentials.ShouldHaveSingleItem().Key.ShouldBeOfType<X509SecurityKey>();
        signingKey.Certificate.Thumbprint.ShouldBe(signing.Thumbprint);
        encryptionKey.Certificate.Thumbprint.ShouldBe(encryption.Thumbprint);
        signingKey.Certificate.HasPrivateKey.ShouldBeTrue();
        encryptionKey.Certificate.HasPrivateKey.ShouldBeTrue();
    }

    [Fact(DisplayName = "AddIdentityProvider should validate tokens across replicas when certificates are shared")]
    public async Task AddIdentityProvider_Should_ValidateTokensAcrossReplicas_When_CertificatesAreShared()
    {
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        var configuration = CreateConfiguration(signing, encryption);
        var issuer = CreateServerOptions(configuration, "Production");
        var replica = CreateServerOptions(configuration, "Production");
        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "https://localhost/",
            Audience = "tactical-heroes-api",
            Subject = new ClaimsIdentity([new Claim("sub", "certificate-test-user")]),
            SigningCredentials = issuer.SigningCredentials.ShouldHaveSingleItem(),
            EncryptingCredentials = issuer.EncryptionCredentials.ShouldHaveSingleItem()
        });

        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = "https://localhost/",
            ValidAudience = "tactical-heroes-api",
            IssuerSigningKeys = replica.SigningCredentials.Select(credential => credential.Key),
            TokenDecryptionKeys = replica.EncryptionCredentials.Select(credential => credential.Key)
        });

        result.IsValid.ShouldBeTrue();
        result.ClaimsIdentity.FindFirst("sub")?.Value.ShouldBe("certificate-test-user");
    }

    [Fact(DisplayName = "AddIdentityProvider should register both certificate pairs when rotating certificates")]
    public void AddIdentityProvider_Should_RegisterBothCertificatePairs_When_RotatingCertificates()
    {
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        using var nextSigning = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var nextEncryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        var configuration = CreateConfiguration(signing, encryption);
        AddCertificate(configuration, "Identity:Provider:SigningCertificates:1", nextSigning);
        AddCertificate(configuration, "Identity:Provider:EncryptionCertificates:1", nextEncryption);

        var options = CreateServerOptions(configuration, "Production");

        options.SigningCredentials.Select(credential => credential.Key.KeyId)
            .ShouldBe([signing.Thumbprint, nextSigning.Thumbprint], ignoreOrder: true);
        options.EncryptionCredentials.Select(credential => credential.Key.KeyId)
            .ShouldBe([encryption.Thumbprint, nextEncryption.Thumbprint], ignoreOrder: true);
    }

    [Theory(DisplayName = "AddIdentityProvider should reject missing certificates when environment requires certificates")]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void AddIdentityProvider_Should_RejectMissingCertificates_When_EnvironmentRequiresCertificates(string environment)
    {
        var exception = Should.Throw<InvalidOperationException>(() =>
            CreateServerOptions(new Dictionary<string, string?>(), environment));

        exception.Message.ShouldContain("SigningCertificates");
        exception.Message.ShouldContain("EncryptionCertificates");
    }

    [Theory(DisplayName = "AddIdentityProvider should use development certificates when local configuration has no certificates")]
    [InlineData("Development")]
    [InlineData("Test")]
    [InlineData(null)]
    public void AddIdentityProvider_Should_UseDevelopmentCertificates_When_LocalConfigurationHasNoCertificates(string? environment)
    {
        var options = CreateServerOptions(new Dictionary<string, string?>(), environment);

        options.SigningCredentials.ShouldHaveSingleItem().Key.ShouldBeOfType<X509SecurityKey>()
            .Certificate.HasPrivateKey.ShouldBeTrue();
        options.EncryptionCredentials.ShouldHaveSingleItem().Key.ShouldBeOfType<X509SecurityKey>()
            .Certificate.HasPrivateKey.ShouldBeTrue();
    }

    [Theory(DisplayName = "AddIdentityProvider should reject partial configuration when one certificate is missing")]
    [InlineData(SigningPrefix)]
    [InlineData(EncryptionPrefix)]
    public void AddIdentityProvider_Should_RejectPartialConfiguration_When_OneCertificateIsMissing(string prefix)
    {
        using var certificate = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        var configuration = new Dictionary<string, string?>();
        AddCertificate(configuration, prefix, certificate);

        var exception = Should.Throw<InvalidOperationException>(() => CreateServerOptions(configuration, "Development"));

        exception.Message.ShouldContain("must both be configured");
    }

    [Theory(DisplayName = "AddIdentityProvider should reject invalid certificate when data or password is invalid")]
    [InlineData("PfxBase64", "invalid-pfx-sensitive-value")]
    [InlineData("Password", "incorrect-sensitive-password")]
    public void AddIdentityProvider_Should_RejectInvalidCertificate_When_DataOrPasswordIsInvalid(string property, string value)
    {
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        var configuration = CreateConfiguration(signing, encryption);
        configuration[$"{SigningPrefix}:{property}"] = value;

        var exception = Should.Throw<InvalidOperationException>(() => CreateServerOptions(configuration, "Development"));

        exception.Message.ShouldContain(SigningPrefix);
        exception.ToString().ShouldNotContain(value);
    }

    [Fact(DisplayName = "AddIdentityProvider should reject public only certificate when private key is missing")]
    public void AddIdentityProvider_Should_RejectPublicOnlyCertificate_When_PrivateKeyIsMissing()
    {
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var publicSigning = X509CertificateLoader.LoadCertificate(signing.Export(X509ContentType.Cert));
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        var configuration = CreateConfiguration(publicSigning, encryption);

        var exception = Should.Throw<InvalidOperationException>(() => CreateServerOptions(configuration, "Production"));

        exception.Message.ShouldContain("private key");
    }

    private static Dictionary<string, string?> CreateConfiguration(X509Certificate2 signing, X509Certificate2 encryption)
    {
        var configuration = new Dictionary<string, string?>();
        AddCertificate(configuration, SigningPrefix, signing);
        AddCertificate(configuration, EncryptionPrefix, encryption);
        return configuration;
    }

    private static void AddCertificate(Dictionary<string, string?> configuration, string prefix, X509Certificate2 certificate)
    {
        configuration[$"{prefix}:PfxBase64"] = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, CertificatePassword));
        configuration[$"{prefix}:Password"] = CertificatePassword;
    }

    private static OpenIddictServerOptions CreateServerOptions(Dictionary<string, string?> values, string? environment)
    {
        values["Identity:Provider:Issuer"] = "https://localhost/";
        values["Identity:Provider:Audience"] = "tactical-heroes-api";
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = environment ?? "Test"
        });
        builder.Services.AddIdentityProvider(configuration, environment is null ? null : builder.Environment);
        using var provider = builder.Services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<OpenIddictServerOptions>>().Value;
    }

    private static X509Certificate2 CreateCertificate(X509KeyUsageFlags usage)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=OpenIddict Test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(usage, critical: true));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
    }
}
