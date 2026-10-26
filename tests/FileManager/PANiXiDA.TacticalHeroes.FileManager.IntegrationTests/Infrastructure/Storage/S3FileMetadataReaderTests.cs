using System.Net;

using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Infrastructure.Storage.S3.Configuration;

using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Storage;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Storage;

public sealed class S3FileMetadataReaderTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Storage metadata should match uploaded content when module key prefix is configured")]
    public async Task GetMetadataAsync_Should_MatchUploadedContent_When_ModuleKeyPrefixIsConfigured()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        await using var content = new MemoryStream([1, 2, 3, 4]);
        await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .UploadAsync("personal/test.txt", content, "text/plain", cancellationToken);

        var metadata = await scope.ServiceProvider.GetRequiredService<S3FileMetadataReader>()
            .GetMetadataAsync("personal/test.txt", cancellationToken);

        metadata.ShouldNotBeNull();
        metadata.ContentType.ShouldBe("text/plain");
        metadata.Size.ShouldBe(4);
    }

    [Fact(DisplayName = "Storage metadata should return no result when object is missing")]
    public async Task GetMetadataAsync_Should_ReturnNoResult_When_ObjectIsMissing()
    {
        await using var scope = Fixture.CreateScope();

        var metadata = await scope.ServiceProvider.GetRequiredService<S3FileMetadataReader>()
            .GetMetadataAsync("personal/missing.txt", TestContext.Current.CancellationToken);

        metadata.ShouldBeNull();
    }

    [Theory(DisplayName = "Storage metadata should preserve failures when storage response is ambiguous")]
    [InlineData(HttpStatusCode.Forbidden, "AccessDenied")]
    [InlineData(HttpStatusCode.InternalServerError, "InternalError")]
    [InlineData(HttpStatusCode.NotFound, "NoSuchBucket")]
    public async Task GetMetadataAsync_Should_PreserveFailures_When_StorageResponseIsAmbiguous(
        HttpStatusCode statusCode,
        string errorCode)
    {
        var client = Substitute.For<IAmazonS3>();
        var failure = new AmazonS3Exception("Storage failed") { StatusCode = statusCode, ErrorCode = errorCode };
        client.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(failure);
        var reader = new S3FileMetadataReader(
            client,
            Options.Create(new S3StorageOptions { BucketName = "test-bucket" }));

        var thrown = await Should.ThrowAsync<AmazonS3Exception>(() =>
            reader.GetMetadataAsync("file.txt", TestContext.Current.CancellationToken));

        thrown.ShouldBeSameAs(failure);
    }
}
