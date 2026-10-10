using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
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
}
