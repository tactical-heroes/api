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
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreateUploadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.CreateUploadUrl;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class CreateFileUploadUrlEndpointTests(ManualReconciliationTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files/upload-url";

    [Theory(DisplayName = "Upload URL endpoint should support direct upload when creation is permitted")]
    [InlineData("Personal", false, false)]
    [InlineData("Personal", false, true)]
    [InlineData("Personal", true, true)]
    [InlineData("Avatar", false, false)]
    [InlineData("Avatar", false, true)]
    public async Task HandleAsync_Should_SupportDirectUpload_When_CreationIsPermitted(
        string type, bool foreignOwner, bool inFolder)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : actorId : null;
        var folder = inFolder ? await AddFolderAsync(type, ownerId) : null;
        byte[] bytes = [1, 2, 3, 4];
        var request = new CreateFileUploadUrlRequest(
            Name: " отчёт 2026.txt ",
            Type: type,
            ContentType: "TEXT/PLAIN",
            Size: bytes.Length,
            UserId: ownerId,
            FolderId: folder?.Id.Value);
        var permission = type == "Avatar" ? "file-manager.avatars.create"
            : foreignOwner ? "file-manager.personal-files.create.all" : "file-manager.personal-files.create.own";
        var before = DateTimeOffset.UtcNow;

        using var response = await SendAsync(request, actorId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var upload = await response.Content.ReadFromJsonAsync<CreateFileUploadUrlResponse>(cancellationToken);
        upload.ShouldNotBeNull();
        upload.Id.ShouldNotBe(Guid.Empty);
        response.Headers.Location.ShouldNotBeNull();
        new Uri(Fixture.Client.BaseAddress!, response.Headers.Location).AbsolutePath
            .ShouldBe($"/api/v1/files/{upload.Id}");
        upload.ExpiresAt.ShouldBeGreaterThan(before.AddMinutes(14));
        upload.ExpiresAt.ShouldBeLessThan(DateTimeOffset.UtcNow.AddMinutes(16));
        upload.RequiredHeaders["Content-Type"].ShouldBe("text/plain");
        var uri = new Uri(upload.Url);
        uri.GetLeftPart(UriPartial.Authority).ShouldBe(Fixture.Storage.ServiceUrl);
        uri.Query.ShouldContain("X-Amz-Signature=");
        var pending = await ReadAsync(upload.Id);
        pending.Name.ShouldBe("отчёт 2026.txt");
        pending.Type.ShouldBe(type);
        pending.UserId.ShouldBe(ownerId);
        pending.FolderId.ShouldBe(folder?.Id.Value);
        pending.Status.ShouldBe("PendingUpload");
        pending.ContentType.ShouldBeNull();
        pending.Size.ShouldBeNull();
        await using (var scope = Fixture.CreateScope())
        {
            var metadata = await scope.ServiceProvider.GetRequiredService<IFileStorage>()
                .GetMetadataAsync(pending.StorageKey, cancellationToken);
            metadata.ShouldBeNull();
        }
        using var client = new HttpClient();
        using var put = new HttpRequestMessage(HttpMethod.Put, upload.Url);
        put.Content = new ByteArrayContent(bytes);
        foreach (var header in upload.RequiredHeaders)
        {
            put.Content.Headers.TryAddWithoutValidation(header.Key, header.Value).ShouldBeTrue();
        }
        using var uploaded = await client.SendAsync(put, cancellationToken);
        uploaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync(upload.Id)).Status.ShouldBe("PendingUpload");
        await ReconcileAsync(upload.Id, DateTimeOffset.UtcNow.AddMinutes(-16).UtcDateTime);
        var ready = await ReadAsync(upload.Id);
        ready.Status.ShouldBe("Ready");
        ready.ContentType.ShouldBe("text/plain");
        ready.Size.ShouldBe(bytes.Length);
        var readPermission = type == "Avatar"
            ? "file-manager.avatars.read" : "file-manager.personal-files.read.all";
        using var downloadRequest = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/files/{upload.Id}/download-url");
        downloadRequest.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        downloadRequest.Headers.Add(TestAuthenticationHandler.PermissionsHeader, readPermission);
        using var downloadResponse = await Fixture.Client.SendAsync(downloadRequest, cancellationToken);
        downloadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var download = await downloadResponse.Content.ReadFromJsonAsync<GetFileDownloadUrlResponse>(cancellationToken);
        download.ShouldNotBeNull();
        using var downloaded = await client.GetAsync(download.Url, cancellationToken);
        downloaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await downloaded.Content.ReadAsByteArrayAsync(cancellationToken)).ShouldBe(bytes);
    }

    [Theory(DisplayName = "Upload URL endpoint should deny creation when permission does not cover the file")]
    [InlineData("Personal", false, "")]
    [InlineData("Personal", false, "file-manager.personal-files.read.all")]
    [InlineData("Personal", true, "file-manager.personal-files.create.own")]
    [InlineData("Personal", false, "file-manager.avatars.create")]
    [InlineData("Avatar", false, "file-manager.personal-files.create.all")]
    [InlineData("Avatar", false, "file-manager.avatars.read")]
    public async Task HandleAsync_Should_DenyCreation_When_PermissionDoesNotCoverTheFile(
        string type, bool foreignOwner, string permission)
    {
        var actorId = Guid.NewGuid();
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : actorId : null;
        var request = new CreateFileUploadUrlRequest("file.txt", type, "text/plain", 4, ownerId);

        using var response = await SendAsync(request, actorId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Upload URL endpoint should reject metadata when request is invalid")]
    [InlineData("name")]
    [InlineData("type")]
    [InlineData("content-type")]
    [InlineData("size")]
    [InlineData("negative-size")]
    [InlineData("owner")]
    [InlineData("folder")]
    [InlineData("missing-owner")]
    [InlineData("avatar-owner")]
    public async Task HandleAsync_Should_RejectMetadata_When_RequestIsInvalid(string scenario)
    {
        var actorId = Guid.NewGuid();
        var request = new CreateFileUploadUrlRequest("file.txt", "Personal", "text/plain", 4, actorId);
        request = scenario switch
        {
            "name" => request with { Name = "../file.txt" },
            "type" => request with { Type = "Unknown" },
            "content-type" => request with { ContentType = "text/*" },
            "size" => request with { Size = 0 },
            "negative-size" => request with { Size = -1 },
            "owner" => request with { UserId = Guid.Empty },
            "folder" => request with { FolderId = Guid.Empty },
            "missing-owner" => request with { UserId = null },
            _ => request with { Type = "Avatar" }
        };

        using var response = await SendAsync(
            request, actorId.ToString(), "file-manager.personal-files.create.all,file-manager.avatars.create");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Upload URL endpoint should reject destination when folder is unavailable or incompatible")]
    [InlineData("missing", HttpStatusCode.NotFound)]
    [InlineData("deleted", HttpStatusCode.NotFound)]
    [InlineData("foreign", HttpStatusCode.NotFound)]
    [InlineData("type", HttpStatusCode.BadRequest)]
    [InlineData("owner", HttpStatusCode.BadRequest)]
    public async Task HandleAsync_Should_RejectDestination_When_FolderIsUnavailableOrIncompatible(
        string scenario, HttpStatusCode expected)
    {
        var actorId = Guid.NewGuid();
        var folderType = scenario == "type" ? "Avatar" : "Personal";
        Guid? folderOwner = folderType == "Avatar" ? null
            : scenario is "foreign" or "owner" ? Guid.NewGuid() : actorId;
        var folder = await AddFolderAsync(folderType, folderOwner, deleted: scenario == "deleted");
        var request = new CreateFileUploadUrlRequest(
            Name: "file.txt",
            Type: "Personal",
            ContentType: "text/plain",
            Size: 4,
            UserId: actorId,
            FolderId: scenario == "missing" ? Guid.NewGuid() : folder.Id.Value);
        var permission = scenario == "foreign" ? "file-manager.personal-files.create.own"
            : "file-manager.personal-files.create.all,file-manager.avatars.create";

        using var response = await SendAsync(request, actorId.ToString(), permission);

        response.StatusCode.ShouldBe(expected);
        await AssertNoFilesAsync();
    }

    [Fact(DisplayName = "Upload URL endpoint should require authentication when caller is anonymous")]
    public async Task HandleAsync_Should_RequireAuthentication_When_CallerIsAnonymous()
    {
        var request = new CreateFileUploadUrlRequest("file.txt", "Avatar", "text/plain", 4);

        using var response = await SendAsync(request, userId: null, permission: "file-manager.avatars.create");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await AssertNoFilesAsync();
    }

    [Fact(DisplayName = "Upload URL endpoint should expire reservation when content is never uploaded")]
    public async Task HandleAsync_Should_ExpireReservation_When_ContentIsNeverUploaded()
    {
        var actorId = Guid.NewGuid();
        using var response = await SendAsync(
            request: new CreateFileUploadUrlRequest("file.txt", "Personal", "text/plain", 4, actorId),
            userId: actorId.ToString(),
            permission: "file-manager.personal-files.create.own");
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var upload = await response.Content.ReadFromJsonAsync<CreateFileUploadUrlResponse>(
            TestContext.Current.CancellationToken);
        upload.ShouldNotBeNull();

        await ReconcileAsync(upload.Id, DateTimeOffset.UtcNow.AddDays(-2).UtcDateTime);

        await using var scope = Fixture.CreateScope();
        var stored = await scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .IgnoreQueryFilters()
            .SingleAsync(file => file.Id == upload.Id, TestContext.Current.CancellationToken);
        stored.Status.ShouldBe("Deleting");
        stored.DeletedAt.ShouldNotBeNull();
    }

    private async Task<HttpResponseMessage> SendAsync(
        CreateFileUploadUrlRequest request, string? userId, string permission)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, Route);
        if (userId is not null)
        {
            message.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId);
        }
        message.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        message.Content = JsonContent.Create(request);
        return await Fixture.Client.SendAsync(message, TestContext.Current.CancellationToken);
    }

    private async Task<Folder> AddFolderAsync(string type, Guid? ownerId, bool deleted = false)
    {
        var folder = Folder.Create(
            FolderName.Create("Folder").Value, FileType.FromName(type), UserId.Create(ownerId).Value).Value;
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

    private async Task ReconcileAsync(Guid id, DateTime createdAt)
    {
        await using (var scope = Fixture.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>().Database
                .ExecuteSqlInterpolatedAsync(
                    $"UPDATE file_manager.files SET created_at = {createdAt} WHERE id = {id}",
                    TestContext.Current.CancellationToken);
        }
        await using var mediatorScope = Fixture.CreateScope();
        var result = await mediatorScope.ServiceProvider.GetRequiredService<IMediator>()
            .SendAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }
}
