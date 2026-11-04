using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonalUploadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.CreatePersonalUploadUrl;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class CreatePersonalFileUploadUrlEndpointTests(ManualReconciliationTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files/personal/upload-url";

    [Theory(DisplayName = "Personal upload URL endpoint should preserve scope when client supplies a different scope")]
    [InlineData("file-manager.personal-files.create.own", false)]
    [InlineData("file-manager.personal-files.create.own", true)]
    [InlineData("file-manager.personal-files.create.all", false)]
    [InlineData("file-manager.personal-files.create.all", true)]
    public async Task HandleAsync_Should_PreserveScope_When_ClientSuppliesADifferentScope(
        string permission, bool inFolder)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var folder = inFolder ? await AddFolderAsync(FileType.Personal, userId) : null;
        var request = new CreatePersonalFileUploadUrlRequest(" отчёт.txt ", "TEXT/PLAIN", 4, folder?.Id.Value);

        using var response = await SendAsync(request, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var upload = await response.Content.ReadFromJsonAsync<CreatePersonalFileUploadUrlResponse>(cancellationToken);
        upload.ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull();
        new Uri(Fixture.Client.BaseAddress!, response.Headers.Location).AbsolutePath
            .ShouldBe($"/api/v1/files/{upload.Id}");
        var pending = await ReadAsync(upload.Id);
        pending.Name.ShouldBe("отчёт.txt");
        pending.Type.ShouldBe("Personal");
        pending.UserId.ShouldBe(userId);
        pending.FolderId.ShouldBe(folder?.Id.Value);
        pending.Status.ShouldBe("PendingUpload");
        pending.ContentType.ShouldBeNull();
        pending.Size.ShouldBeNull();
        upload.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        upload.RequiredHeaders["Content-Type"].ShouldBe("text/plain");
        var uri = new Uri(upload.Url);
        uri.GetLeftPart(UriPartial.Authority).ShouldBe(Fixture.Storage.ServiceUrl);
        uri.Query.ShouldContain("X-Amz-Signature=");
        await using (var scope = Fixture.CreateScope())
        {
            var metadata = await scope.ServiceProvider.GetRequiredService<IFileStorage>()
                .GetMetadataAsync(pending.StorageKey, cancellationToken);
            metadata.ShouldBeNull();
        }
        using var client = new HttpClient();
        using var put = new HttpRequestMessage(HttpMethod.Put, upload.Url);
        put.Content = new ByteArrayContent([1, 2, 3, 4]);
        foreach (var header in upload.RequiredHeaders)
        {
            put.Content.Headers.TryAddWithoutValidation(header.Key, header.Value).ShouldBeTrue();
        }
        using var uploaded = await client.SendAsync(put, cancellationToken);
        uploaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync(upload.Id)).Status.ShouldBe("PendingUpload");
        await ReconcileAsync(upload.Id);
        var ready = await ReadAsync(upload.Id);
        ready.Status.ShouldBe("Ready");
        ready.UserId.ShouldBe(userId);
        ready.Type.ShouldBe("Personal");
        ready.ContentType.ShouldBe("text/plain");
        ready.Size.ShouldBe(4);
        using var downloadRequest = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/files/{upload.Id}/download-url");
        downloadRequest.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        downloadRequest.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader, "file-manager.personal-files.read.own");
        using var downloadResponse = await Fixture.Client.SendAsync(downloadRequest, cancellationToken);
        downloadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var download = await downloadResponse.Content.ReadFromJsonAsync<GetFileDownloadUrlResponse>(cancellationToken);
        download.ShouldNotBeNull();
        using var downloaded = await client.GetAsync(download.Url, cancellationToken);
        downloaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await downloaded.Content.ReadAsByteArrayAsync(cancellationToken)).ShouldBe([1, 2, 3, 4]);
    }

    [Theory(DisplayName = "Personal upload URL endpoint should require authentication when subject is unavailable")]
    [InlineData(null)]
    [InlineData("invalid-subject")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task HandleAsync_Should_RequireAuthentication_When_SubjectIsUnavailable(string? subject)
    {
        using var response = await SendAsync(
            request: new CreatePersonalFileUploadUrlRequest("file.txt", "text/plain", 4),
            subject: subject,
            permission: "file-manager.personal-files.create.own");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Personal upload URL endpoint should deny creation when create permission is missing")]
    [InlineData("")]
    [InlineData("file-manager.personal-files.read.own")]
    [InlineData("file-manager.avatars.create")]
    public async Task HandleAsync_Should_DenyCreation_When_CreatePermissionIsMissing(string permission)
    {
        using var response = await SendAsync(
            new CreatePersonalFileUploadUrlRequest("file.txt", "text/plain", 4), Guid.NewGuid().ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Personal upload URL endpoint should reject destination when folder cannot contain the file")]
    [InlineData("missing", HttpStatusCode.NotFound)]
    [InlineData("deleted", HttpStatusCode.NotFound)]
    [InlineData("foreign", HttpStatusCode.NotFound)]
    [InlineData("avatar", HttpStatusCode.NotFound)]
    [InlineData("foreign-all", HttpStatusCode.BadRequest)]
    [InlineData("empty", HttpStatusCode.BadRequest)]
    public async Task HandleAsync_Should_RejectDestination_When_FolderCannotContainTheFile(
        string scenario, HttpStatusCode expected)
    {
        var userId = Guid.NewGuid();
        var type = scenario == "avatar" ? FileType.Avatar : FileType.Personal;
        Guid? ownerId = type == FileType.Avatar ? null
            : scenario is "foreign" or "foreign-all" ? Guid.NewGuid() : userId;
        var folder = await AddFolderAsync(type, ownerId, deleted: scenario == "deleted");
        var folderId = scenario == "empty" ? Guid.Empty : scenario == "missing" ? Guid.NewGuid() : folder.Id.Value;
        var request = new CreatePersonalFileUploadUrlRequest("file.txt", "text/plain", 4, folderId);
        var permission = scenario == "foreign-all"
            ? "file-manager.personal-files.create.all" : "file-manager.personal-files.create.own";

        using var response = await SendAsync(request, userId.ToString(), permission);

        response.StatusCode.ShouldBe(expected);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Personal upload URL endpoint should reject metadata when request is invalid")]
    [InlineData("name")]
    [InlineData("content-type")]
    [InlineData("size")]
    public async Task HandleAsync_Should_RejectMetadata_When_RequestIsInvalid(string scenario)
    {
        var request = new CreatePersonalFileUploadUrlRequest(
            scenario == "name" ? "../file.txt" : "file.txt",
            scenario == "content-type" ? "text/*" : "text/plain",
            scenario == "size" ? 0 : 4);

        using var response = await SendAsync(
            request, Guid.NewGuid().ToString(), "file-manager.personal-files.create.own");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertNoFilesAsync();
    }

    private async Task<HttpResponseMessage> SendAsync(
        CreatePersonalFileUploadUrlRequest request, string? subject, string permission)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, Route);
        if (subject is not null)
        {
            message.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        message.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        message.Content = JsonContent.Create(
            new
            {
                request.Name,
                request.ContentType,
                request.Size,
                request.FolderId,
                Type = "Avatar",
                UserId = Guid.NewGuid()
            });
        return await Fixture.Client.SendAsync(message, TestContext.Current.CancellationToken);
    }

    private async Task<Folder> AddFolderAsync(FileType type, Guid? ownerId, bool deleted = false)
    {
        var folder = Folder.Create(
            FolderName.Create("Folder").Value, type, UserId.Create(ownerId).Value).Value;
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(folder);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            db.Remove(folder);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return folder;
    }

    private async Task<FileReadDbModel> ReadAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .SingleAsync(file => file.Id == id, TestContext.Current.CancellationToken);
    }

    private async Task AssertNoFilesAsync()
    {
        await using var scope = Fixture.CreateScope();
        var count = await scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .IgnoreQueryFilters()
            .CountAsync(TestContext.Current.CancellationToken);
        count.ShouldBe(0);
    }

    private async Task ReconcileAsync(Guid id)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Fixture.CreateScope())
        {
            var createdAt = DateTimeOffset.UtcNow.AddMinutes(-16).UtcDateTime;
            await scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>().Database
                .ExecuteSqlInterpolatedAsync(
                    $"UPDATE file_manager.files SET created_at = {createdAt} WHERE id = {id}", cancellationToken);
        }
        await using var mediatorScope = Fixture.CreateScope();
        var result = await mediatorScope.ServiceProvider.GetRequiredService<IMediator>()
            .SendAsync(new ReconcileFileUploadsCommand(), cancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }
}
