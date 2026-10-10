using System.Security.Cryptography;

using Amazon.S3.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonal;
using PANiXiDA.TacticalHeroes.Testing.Storage;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.CreatePersonal;

public sealed class CreatePersonalFileEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files/personal";

    [Theory(DisplayName = "Create personal file endpoint should persist metadata and upload bytes when request is valid")]
    [InlineData(32, false)]
    [InlineData(32, true)]
    [InlineData(17 * 1024 * 1024, false)]
    public async Task HandleAsync_Should_CreateReadyFileInS3_When_RequestIsValid(int size, bool hasFolder)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        Guid? folderId = hasFolder ? await CreateFolderAsync(userId) : null;
        var bytes = new byte[size];
        Random.Shared.NextBytes(bytes);
        using var request = CreateRequest(userId.ToString(), bytes, "file.bin", "application/octet-stream", folderId);
        ((MultipartFormDataContent)request.Content!).Add(new StringContent(Guid.NewGuid().ToString()), "UserId");

        using var response = await Fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(cancellationToken));
        var body = await response.Content.ReadFromJsonAsync<CreatePersonalFileResponse>(cancellationToken);
        body.ShouldNotBeNull();
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var file = await db.Set<FileReadDbModel>().SingleAsync(cancellationToken);
        file.Id.ShouldBe(body.Id);
        file.Name.ShouldBe("file.bin");
        file.Type.ShouldBe("Personal");
        file.UserId.ShouldBe(userId);
        file.FolderId.ShouldBe(folderId);
        file.Status.ShouldBe("Ready");
        file.Size.ShouldBe(size);
        file.ContentType.ShouldBe("application/octet-stream");
        file.StorageKey.ShouldBe($"personal/{userId:D}/{body.Id:D}");

        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        await using var downloaded = await storage.DownloadAsync(file.StorageKey, cancellationToken);
        var downloadedHash = await SHA256.HashDataAsync(downloaded, cancellationToken);
        downloadedHash.ShouldBe(SHA256.HashData(bytes));

        using var s3 = Fixture.Storage.CreateClient();
        var metadata = await s3.GetObjectMetadataAsync(new GetObjectMetadataRequest
        {
            BucketName = S3TestStorage.BucketName,
            Key = $"{S3TestStorage.KeyPrefix}/{file.StorageKey}"
        }, cancellationToken);
        metadata.ContentLength.ShouldBe(size);
        metadata.Headers.ContentType.ShouldBe(file.ContentType);
    }

    [Theory(DisplayName = "Create personal file endpoint should reject authentication when subject is unavailable")]
    [InlineData(null)]
    [InlineData("invalid-subject")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_SubjectIsUnavailable(string? subject)
    {
        using var request = CreateRequest(subject, [1], "file.bin", "application/octet-stream", folderId: null);

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Create personal file endpoint should reject invalid metadata when file is invalid")]
    [InlineData("../file.txt", "text/plain", 1)]
    [InlineData("file.txt", "text/*", 1)]
    [InlineData("file.txt", "text/plain", 0)]
    public async Task HandleAsync_Should_ReturnBadRequest_When_FileIsInvalid(string name, string contentType, int size)
    {
        using var request = CreateRequest(Guid.NewGuid().ToString(), new byte[size], name, contentType, folderId: null);

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertNoFilesAsync();
    }

    [Fact(DisplayName = "Create personal file endpoint should reject the request when file is missing")]
    public async Task HandleAsync_Should_ReturnBadRequest_When_FileIsMissing()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Route);
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, Guid.NewGuid().ToString());
        request.Content = new MultipartFormDataContent
        {
            { new StringContent("ignored"), "Name" }
        };

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Create personal file endpoint should reject the request when folder cannot be used")]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("avatar")]
    [InlineData("empty")]
    public async Task HandleAsync_Should_RejectFolder_When_FolderCannotBeUsed(string folderKind)
    {
        var folderId = folderKind switch
        {
            "foreign" => await CreateFolderAsync(Guid.NewGuid()),
            "avatar" => await CreateFolderAsync(userId: null),
            "empty" => Guid.Empty,
            _ => Guid.NewGuid()
        };
        using var request = CreateRequest(Guid.NewGuid().ToString(), [1], "file.bin", "application/octet-stream", folderId);

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(folderKind == "empty" ? HttpStatusCode.BadRequest : HttpStatusCode.NotFound);
        await AssertNoFilesAsync();
    }

    private static HttpRequestMessage CreateRequest(
        string? subject,
        byte[] bytes,
        string name,
        string contentType,
        Guid? folderId)
    {
        var multipart = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.TryAddWithoutValidation("Content-Type", contentType);
        multipart.Add(file, "File", name);
        if (folderId is { } id)
        {
            multipart.Add(new StringContent(id.ToString()), "FolderId");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, Route) { Content = multipart };
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }

        return request;
    }

    private async Task<Guid> CreateFolderAsync(Guid? userId)
    {
        await using var scope = Fixture.CreateScope();
        var folder = Folder.Create(
            FolderName.Create("Documents").Value,
            userId is null ? FileType.Avatar : FileType.Personal,
            userId is { } ownerId ? UserId.Create(ownerId).Value : null).Value;
        await scope.ServiceProvider.GetRequiredService<IFoldersRepository>()
            .AddAsync(folder, TestContext.Current.CancellationToken);

        return folder.Id.Value;
    }

    private async Task AssertNoFilesAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        (await db.Set<FileReadDbModel>().AnyAsync(cancellationToken)).ShouldBeFalse();
        using var s3 = Fixture.Storage.CreateClient();
        var response = await s3.ListObjectsV2Async(
            new ListObjectsV2Request { BucketName = S3TestStorage.BucketName },
            cancellationToken);
        (response.S3Objects?.Count ?? 0).ShouldBe(0);
    }
}
