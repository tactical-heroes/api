using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace PANiXiDA.TacticalHeroes.Testing.Storage;

public sealed class S3TestStorage : IAsyncDisposable
{
    public const string BucketName = "file-manager-tests";
    public const string KeyPrefix = "tests";

    private const string AccessKey = "integration-access";
    private const string SecretKey = "integration-secret";

    private readonly IContainer _container = new ContainerBuilder("chrislusf/seaweedfs:4.48")
        .WithCommand("mini", "-dir=/data", "-s3.autoCreateBucket=false")
        .WithPortBinding(8333, assignRandomHostPort: true)
        .WithEnvironment("AWS_ACCESS_KEY_ID", AccessKey)
        .WithEnvironment("AWS_SECRET_ACCESS_KEY", SecretKey)
        .WithEnvironment("S3_BUCKET", BucketName)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilMessageIsLogged("S3 Endpoint:", strategy =>
                strategy.WithTimeout(TimeSpan.FromMinutes(1))))
        .Build();

    public string ServiceUrl => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(8333)}";

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _container.StartAsync(cancellationToken);

        using var client = CreateClient();
        var response = await client.ListBucketsAsync(cancellationToken);
        response.Buckets.ShouldContain(bucket => bucket.BucketName == BucketName);
    }

    public Dictionary<string, string?> GetConfiguration(string sectionName)
    {
        return new Dictionary<string, string?>
        {
            [$"{sectionName}:AWS:ServiceURL"] = ServiceUrl,
            [$"{sectionName}:AWS:AuthenticationRegion"] = "us-east-1",
            [$"{sectionName}:AWS:ForcePathStyle"] = "true",
            [$"{sectionName}:S3Storage:BucketName"] = BucketName,
            [$"{sectionName}:S3Storage:KeyPrefix"] = KeyPrefix,
            [$"{sectionName}:S3Storage:AccessKey"] = AccessKey,
            [$"{sectionName}:S3Storage:SecretKey"] = SecretKey
        };
    }

    public IAmazonS3 CreateClient()
    {
        return new AmazonS3Client(
            new BasicAWSCredentials(AccessKey, SecretKey),
            new AmazonS3Config
            {
                ServiceURL = ServiceUrl,
                AuthenticationRegion = "us-east-1",
                ForcePathStyle = true
            });
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        using var client = CreateClient();

        while (true)
        {
            var response = await client.ListObjectsV2Async(
                new ListObjectsV2Request { BucketName = BucketName },
                cancellationToken);
            if (response.S3Objects is not { Count: > 0 } objects)
            {
                return;
            }

            var deleted = await client.DeleteObjectsAsync(new DeleteObjectsRequest
            {
                BucketName = BucketName,
                Objects = [.. objects.Select(item => new KeyVersion { Key = item.Key })]
            }, cancellationToken);
            (deleted.DeleteErrors?.Count ?? 0).ShouldBe(0);
        }
    }

    public ValueTask DisposeAsync()
    {
        return _container.DisposeAsync();
    }
}
