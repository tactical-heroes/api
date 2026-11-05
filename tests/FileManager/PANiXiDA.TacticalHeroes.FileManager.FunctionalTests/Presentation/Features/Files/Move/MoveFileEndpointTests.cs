using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Move;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Update;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.Move;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class MoveFileEndpointTests(ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";
    private const string AllUpdates = "file-manager.personal-files.update.all,file-manager.avatars.update";

    [Theory(DisplayName = "PATCH file parent should respect update scopes when type and ownership vary")]
    [InlineData("Personal", true, "file-manager.personal-files.update.own", true)]
    [InlineData("Personal", false, "file-manager.personal-files.update.own", false)]
    [InlineData("Personal", false, "file-manager.personal-files.update.all", true)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", false)]
    [InlineData("Personal", true, "file-manager.personal-files.create.own", false)]
    [InlineData("Personal", true, "file-manager.personal-files.delete.all", false)]
    [InlineData("Personal", true, "file-manager.avatars.update", false)]
    [InlineData("Personal", true, "", false)]
    [InlineData("Avatar", false, "file-manager.avatars.update", true)]
    [InlineData("Avatar", false, "file-manager.personal-files.update.all", false)]
    [InlineData("Avatar", false, "file-manager.avatars.read", false)]
    public async Task HandleAsync_Should_RespectUpdateScopes_When_TypeAndOwnershipVary(
        string type, bool own, string permission, bool allowed)
    {
        var actorId = Guid.NewGuid();
        var ownerId = own ? actorId : Guid.NewGuid();
        var file = CreateFile(type, ownerId);
        var folder = CreateFolder(type, ownerId);
        await SaveAsync(file, folder);

        using var response = await SendAsync(
            file.Id.Value, new MoveFileRequest(folder.Id.Value), actorId, permission);

        response.StatusCode.ShouldBe(allowed ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
        var stored = await ReadAsync(file.Id.Value);
        stored.FolderId.ShouldBe(allowed ? folder.Id.Value : null);
        stored.Type.ShouldBe(type);
        stored.UserId.ShouldBe(file.UserId?.Value);
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
        stored.Status.ShouldBe("PendingUpload");
        stored.Name.ShouldBe("file.txt");
    }

    [Theory(DisplayName = "PATCH file parent should preserve content and folders when moving a ready file")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_PreserveContentAndFolders_When_MovingAReadyFile(string type)
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        var file = CreateFile(type, actorId);
        var source = CreateFolder(type, actorId);
        var target = CreateFolder(type, actorId);
        file.MoveTo(source);
        byte[] bytes = [1, 2, 3, 4];
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(bytes.Length).Value);
        await SaveAsync(file, source, target);
        uint[] versions;
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            versions = await db.Set<Folder>()
                .OrderBy(folder => folder.Id)
                .Select(folder => EF.Property<uint>(folder, "xmin"))
                .ToArrayAsync(ct);
            await using var content = new MemoryStream(bytes);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>().UploadAsync(
                file.StorageKey.Value, content, "text/plain", ct);
        }

        using var response = await SendAsync(
            file.Id.Value, new MoveFileRequest(target.Id.Value), actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsByteArrayAsync(ct)).ShouldBeEmpty();
        var stored = await ReadAsync(file.Id.Value);
        stored.FolderId.ShouldBe(target.Id.Value);
        stored.Name.ShouldBe("file.txt");
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
        stored.UserId.ShouldBe(file.UserId?.Value);
        stored.Type.ShouldBe(type);
        stored.Status.ShouldBe("Ready");
        stored.ContentType.ShouldBe("text/plain");
        stored.Size.ShouldBe(bytes.Length);
        await using var verification = Fixture.CreateScope();
        var write = verification.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var savedVersions = await write.Set<Folder>()
            .OrderBy(folder => folder.Id)
            .Select(folder => EF.Property<uint>(folder, "xmin"))
            .ToArrayAsync(ct);
        savedVersions.ShouldBe(versions);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{file.Id.Value}/download-url");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        request.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            type == "Personal" ? "file-manager.personal-files.read.own" : "file-manager.avatars.read");
        using var downloadResponse = await Fixture.Client.SendAsync(request, ct);
        downloadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var download = await downloadResponse.Content.ReadFromJsonAsync<GetFileDownloadUrlResponse>(ct);
        download.ShouldNotBeNull();
        using var client = new HttpClient();
        using var downloaded = await client.GetAsync(download.Url, ct);
        downloaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await downloaded.Content.ReadAsByteArrayAsync(ct)).ShouldBe(bytes);
        downloaded.Content.Headers.ContentDisposition!.FileNameStar.ShouldBe("file.txt");
    }

    [Theory(DisplayName = "PATCH file parent should detach the file when folder is null or omitted")]
    [InlineData("Personal", false)]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    [InlineData("Avatar", true)]
    public async Task HandleAsync_Should_DetachFile_When_FolderIsNullOrOmitted(string type, bool omitted)
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile(type, actorId);
        var folder = CreateFolder(type, actorId);
        file.MoveTo(folder);
        await SaveAsync(file, folder);
        object body = omitted ? new { } : new MoveFileRequest(FolderId: null);

        using var response = await SendAsync(file.Id.Value, body, actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await ReadAsync(file.Id.Value);
        stored.FolderId.ShouldBeNull();
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
        stored.UserId.ShouldBe(file.UserId?.Value);
    }

    [Theory(DisplayName = "PATCH file parent should preserve placement when request is unavailable or invalid")]
    [InlineData("missing-file", 404)]
    [InlineData("deleted-file", 404)]
    [InlineData("missing-folder", 404)]
    [InlineData("deleted-folder", 404)]
    [InlineData("foreign-folder", 404)]
    [InlineData("foreign-file", 404)]
    [InlineData("empty-file", 400)]
    [InlineData("empty-folder", 400)]
    [InlineData("anonymous", 401)]
    public async Task HandleAsync_Should_PreservePlacement_When_RequestIsUnavailableOrInvalid(string kind, int code)
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile("Personal", kind == "foreign-file" ? Guid.NewGuid() : actorId);
        var folder = CreateFolder("Personal", kind == "foreign-folder" ? Guid.NewGuid() : actorId);
        await SaveAsync(file, folder);
        if (kind.StartsWith("deleted-", StringComparison.Ordinal))
        {
            await using var scope = Fixture.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            if (kind == "deleted-file")
            {
                db.Remove(file);
            }
            else
            {
                db.Remove(folder);
            }
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        var id = kind == "missing-file" ? Guid.NewGuid() : kind == "empty-file" ? Guid.Empty : file.Id.Value;
        var folderId = kind == "missing-folder"
            ? Guid.NewGuid()
            : kind == "empty-folder" ? Guid.Empty : folder.Id.Value;

        using var response = await SendAsync(
            id,
            new MoveFileRequest(folderId),
            kind == "anonymous" ? null : actorId,
            "file-manager.personal-files.update.own");

        ((int)response.StatusCode).ShouldBe(code);
        (await ReadAsync(file.Id.Value)).FolderId.ShouldBeNull();
    }

    [Theory(DisplayName = "PATCH file parent should reject the destination when type or owner differs")]
    [InlineData("Personal", "Avatar", false)]
    [InlineData("Avatar", "Personal", false)]
    [InlineData("Personal", "Personal", true)]
    public async Task HandleAsync_Should_RejectDestination_When_TypeOrOwnerDiffers(
        string fileType, string folderType, bool differentOwner)
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile(fileType, actorId);
        var folder = CreateFolder(folderType, differentOwner ? Guid.NewGuid() : actorId);
        await SaveAsync(file, folder);

        using var response = await SendAsync(
            file.Id.Value, new MoveFileRequest(folder.Id.Value), actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(file.Id.Value)).FolderId.ShouldBeNull();
    }

    [Theory(DisplayName = "PATCH file parent should preserve placement when deletion has started")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreservePlacement_When_DeletionHasStarted(bool detach)
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile("Personal", actorId);
        var folder = CreateFolder("Personal", actorId);
        file.MoveTo(folder);
        file.BeginDeletion();
        await SaveAsync(file, folder);

        using var response = await SendAsync(
            file.Id.Value, new MoveFileRequest(detach ? null : folder.Id.Value), actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadAsync(file.Id.Value)).FolderId.ShouldBe(folder.Id.Value);
    }

    [Fact(DisplayName = "PATCH file parent should accept retries when destination is unchanged")]
    public async Task HandleAsync_Should_AcceptRetries_When_DestinationIsUnchanged()
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile("Personal", actorId);
        var folder = CreateFolder("Personal", actorId);
        await SaveAsync(file, folder);
        var request = new MoveFileRequest(folder.Id.Value);
        using var first = await SendAsync(file.Id.Value, request, actorId, AllUpdates);

        using var retry = await SendAsync(file.Id.Value, request, actorId, AllUpdates);

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        retry.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(file.Id.Value)).FolderId.ShouldBe(folder.Id.Value);
    }

    [Theory(DisplayName = "PATCH file parent should preserve scope consistency when folder update runs concurrently")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreserveScopeConsistency_When_FolderUpdateRunsConcurrently(bool changeType)
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        var file = CreateFile("Personal", actorId);
        var folder = CreateFolder("Personal", actorId);
        await SaveAsync(file, folder);
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await scope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>().AcquireAsync(ct);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/folders/{folder.Id.Value}");
        request.Content = JsonContent.Create(new UpdateFolderRequest(
            "After", changeType ? "Avatar" : "Personal", changeType ? null : Guid.NewGuid()));
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, AllUpdates);
        var update = Fixture.Client.SendAsync(request, ct);
        var move = SendAsync(file.Id.Value, new MoveFileRequest(folder.Id.Value), actorId, AllUpdates);
        bool waiting;

        try
        {
            waiting = await WaitForWritersAsync(db, ct);
        }
        finally
        {
            await transaction.RollbackAsync(ct);
        }
        using var updateResponse = await update;
        using var moveResponse = await move;

        waiting.ShouldBeTrue();
        var stored = await ReadAsync(file.Id.Value);
        if (moveResponse.StatusCode == HttpStatusCode.NoContent)
        {
            updateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            stored.FolderId.ShouldBe(folder.Id.Value);
        }
        else
        {
            moveResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            stored.FolderId.ShouldBeNull();
        }
        var read = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var savedFolder = await read.Set<FolderReadDbModel>().SingleAsync(item => item.Id == folder.Id.Value, ct);
        if (stored.FolderId is not null)
        {
            stored.Type.ShouldBe(savedFolder.AllowedFileType);
            stored.UserId.ShouldBe(savedFolder.UserId);
        }
    }

    private static File CreateFile(string type, Guid ownerId)
    {
        return File.Create(
            FileName.Create("file.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(ownerId).Value : null).Value;
    }

    private static Folder CreateFolder(string type, Guid ownerId)
    {
        return Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(ownerId).Value : null).Value;
    }

    private async Task SaveAsync(File file, params Folder[] folders)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.AddRange(folders);
        db.Add(file);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<FileReadDbModel> ReadAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        return await db.Set<FileReadDbModel>().IgnoreQueryFilters()
            .SingleAsync(file => file.Id == id, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, object body, Guid? actorId, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{Route}/{id}/parent");
        request.Content = JsonContent.Create(body);
        if (actorId is { } value)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, value.ToString());
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<bool> WaitForWritersAsync(
        FileManagerWriteDbContext dbContext, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var waiting = await dbContext.Database.SqlQuery<int>(
                $"""
                SELECT COUNT(*)::int AS "Value"
                FROM pg_locks AS waiting
                JOIN pg_locks AS held
                    ON waiting.database = held.database
                    AND waiting.classid = held.classid
                    AND waiting.objid = held.objid
                    AND waiting.objsubid = held.objsubid
                WHERE waiting.locktype = 'advisory' AND NOT waiting.granted
                    AND held.locktype = 'advisory' AND held.granted AND held.pid = pg_backend_pid()
                """)
                .SingleAsync(cancellationToken);
            if (waiting >= 2)
            {
                return true;
            }
            await Task.Delay(10, cancellationToken);
        }
        return false;
    }
}
