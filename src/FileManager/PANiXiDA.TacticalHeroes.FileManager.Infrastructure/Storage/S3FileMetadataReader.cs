using System.Net;

using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Options;

using PANiXiDA.Core.Infrastructure.Storage.S3.Configuration;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Storage;

internal sealed class S3FileMetadataReader(IAmazonS3 client, IOptions<S3StorageOptions> options)
{
    internal async Task<StoredFileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken)
    {
        var storageOptions = options.Value;
        var objectKey = string.IsNullOrEmpty(storageOptions.KeyPrefix)
            ? key
            : $"{storageOptions.KeyPrefix}/{key}";
        try
        {
            var metadata = await client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest
                {
                    BucketName = storageOptions.BucketName,
                    Key = objectKey
                },
                cancellationToken);

            return new StoredFileMetadata(metadata.Headers.ContentType, metadata.ContentLength);
        }
        catch (AmazonS3Exception exception) when (
            exception.StatusCode == HttpStatusCode.NotFound
                && exception.ErrorCode is "NoSuchKey" or "NotFound" or "404")
        {
            return null;
        }
    }
}
