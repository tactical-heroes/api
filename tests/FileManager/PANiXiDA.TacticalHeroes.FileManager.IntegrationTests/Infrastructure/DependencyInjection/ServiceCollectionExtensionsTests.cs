using System.Globalization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Testing.Storage;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "File storage should use module configuration when infrastructure is registered")]
    public async Task AddInfrastructure_Should_UseModuleConfiguration_When_InfrastructureIsRegistered()
    {
        await using var scope = Fixture.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var file = File.Create(
            FileName.Create("avatar.png").Value,
            FileType.Avatar,
            userId: null).Value;

        var result = await storage.GetPresignedDownloadUrlAsync(
            file.StorageKey.Value,
            file.Name.Value,
            "image/png",
            TestContext.Current.CancellationToken);
        var url = new Uri(result.Url);

        url.GetLeftPart(UriPartial.Authority).ShouldBe(Fixture.Storage.ServiceUrl);
        url.AbsolutePath.ShouldBe($"/{S3TestStorage.BucketName}/{S3TestStorage.KeyPrefix}/{file.StorageKey.Value}");
        url.Query.ShouldContain("X-Amz-Signature=");
    }

    [Theory(DisplayName = "Relocation options should use validated configuration when infrastructure is registered")]
    [InlineData(null, true)]
    [InlineData("2.00:00:00", true)]
    [InlineData("00:00:00", false)]
    [InlineData("-1.00:00:00", false)]
    public void AddInfrastructure_Should_UseValidatedRelocationOptions_When_InfrastructureIsRegistered(
        string? cleanupDelay,
        bool valid)
    {
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgreSqlConnectionString"] = Fixture.ConnectionString
        };
        if (cleanupDelay is not null)
        {
            values[$"{FileRelocationOptions.SectionName}:CleanupDelay"] = cleanupDelay;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .AddInMemoryCollection(Fixture.Storage.GetConfiguration(nameof(FileManager)))
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IStartupValidator>();

        if (!valid)
        {
            Should.Throw<OptionsValidationException>(validator.Validate);
            return;
        }

        validator.Validate();
        var options = provider.GetRequiredService<FileRelocationOptions>();
        options.CleanupDelay.ShouldBe(cleanupDelay is null
            ? TimeSpan.FromDays(7)
            : TimeSpan.Parse(cleanupDelay, CultureInfo.InvariantCulture));
        options.ShouldBeSameAs(provider.GetRequiredService<IOptions<FileRelocationOptions>>().Value);
    }
}
