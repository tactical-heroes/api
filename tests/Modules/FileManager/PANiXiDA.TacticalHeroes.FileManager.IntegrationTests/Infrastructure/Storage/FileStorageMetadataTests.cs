using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Storage;

public sealed class FileStorageMetadataTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Storage metadata should match uploaded content when module key prefix is configured")]
    public async Task GetMetadataAsync_Should_MatchUploadedContent_When_ModuleKeyPrefixIsConfigured()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        await using var content = new MemoryStream([1, 2, 3, 4]);
        await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .UploadAsync("personal/test.txt", content, "text/plain", cancellationToken);

        var metadata = await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .GetMetadataAsync("personal/test.txt", cancellationToken);

        metadata.ShouldNotBeNull();
        metadata.ContentType.ShouldBe("text/plain");
        metadata.Size.ShouldBe(4);
    }

    [Fact(DisplayName = "Storage metadata should return no result when object is missing")]
    public async Task GetMetadataAsync_Should_ReturnNoResult_When_ObjectIsMissing()
    {
        await using var scope = Fixture.CreateScope();

        var metadata = await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .GetMetadataAsync("personal/missing.txt", TestContext.Current.CancellationToken);

        metadata.ShouldBeNull();
    }
}
