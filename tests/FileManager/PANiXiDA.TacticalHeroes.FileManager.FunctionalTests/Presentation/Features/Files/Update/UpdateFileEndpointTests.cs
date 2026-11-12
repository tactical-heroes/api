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
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Update;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Update;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.Update;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class UpdateFileEndpointTests(ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";
    private const string AllUpdates = "file-manager.personal-files.update.all,file-manager.avatars.update";

    [Theory(DisplayName = "PUT file should preserve downloadable content when type and owner change")]
    [InlineData("Personal", "Personal")]
    [InlineData("Personal", "Avatar")]
    [InlineData("Avatar", "Personal")]
    [InlineData("Avatar", "Avatar")]
    public async Task HandleAsync_Should_PreserveDownloadableContent_When_TypeAndOwnerChange(
        string sourceType, string targetType)
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        var ownerId = targetType == "Personal" ? Guid.NewGuid() : (Guid?)null;
        var file = CreateFile(sourceType, actorId);
        var folder = CreateFolder(targetType, ownerId);
        byte[] bytes = [1, 2, 3, 4];
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(bytes.Length).Value);
        await SaveAsync(file, folder);
        await using (var scope = Fixture.CreateScope())
        {
            await using var content = new MemoryStream(bytes);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>().UploadAsync(
                file.StorageKey.Value, content, "text/plain", ct);
        }
        var body = new UpdateFileRequest("after.txt", targetType, ownerId, folder.Id.Value);

        using var response = await SendAsync(file.Id.Value, body, actorId, AllUpdates);
        using var retry = await SendAsync(file.Id.Value, body, actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        retry.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsByteArrayAsync(ct)).ShouldBeEmpty();
        var stored = await ReadAsync(file.Id.Value);
        stored.Name.ShouldBe("after.txt");
        stored.Type.ShouldBe(targetType);
        stored.UserId.ShouldBe(ownerId);
        stored.FolderId.ShouldBe(folder.Id.Value);
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
        stored.Status.ShouldBe("Ready");
        stored.ContentType.ShouldBe("text/plain");
        stored.Size.ShouldBe(bytes.Length);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{file.Id.Value}/download-url");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, (ownerId ?? actorId).ToString());
        request.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            targetType == "Personal" ? "file-manager.personal-files.read.own" : "file-manager.avatars.read");
        using var downloadResponse = await Fixture.Client.SendAsync(request, ct);
        downloadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var download = await downloadResponse.Content.ReadFromJsonAsync<GetFileDownloadUrlResponse>(ct);
        download.ShouldNotBeNull();
        using var client = new HttpClient();
        using var downloaded = await client.GetAsync(download.Url, ct);
        downloaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await downloaded.Content.ReadAsByteArrayAsync(ct)).ShouldBe(bytes);
        downloaded.Content.Headers.ContentDisposition!.FileNameStar.ShouldBe("after.txt");
        if (sourceType == "Personal" && targetType == "Personal")
        {
            using var previousOwnerRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"{Route}/{file.Id.Value}/download-url");
            previousOwnerRequest.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
            previousOwnerRequest.Headers
                .Add(TestAuthenticationHandler.PermissionsHeader, "file-manager.personal-files.read.own");
            using var denied = await Fixture.Client.SendAsync(previousOwnerRequest, ct);
            denied.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }
    }

    [Theory(DisplayName = "PUT file should apply update scopes when type and ownership vary")]
    [InlineData("Personal", true, "file-manager.personal-files.update.own", 204)]
    [InlineData("Personal", false, "file-manager.personal-files.update.own", 404)]
    [InlineData("Personal", false, "file-manager.personal-files.update.all", 204)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", 404)]
    [InlineData("Avatar", false, "file-manager.avatars.update", 204)]
    [InlineData("Avatar", false, "file-manager.personal-files.update.all", 404)]
    public async Task HandleAsync_Should_ApplyUpdateScopes_When_TypeAndOwnershipVary(
        string type, bool own, string permission, int code)
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile(type, own ? actorId : Guid.NewGuid());
        await SaveAsync(file);

        using var response = await SendAsync(
            file.Id.Value, new UpdateFileRequest("after.txt", type, file.UserId?.Value), actorId, permission);

        ((int)response.StatusCode).ShouldBe(code);
        (await ReadAsync(file.Id.Value)).Name.ShouldBe(code == 204 ? "after.txt" : "before.txt");
    }

    [Theory(DisplayName = "PUT file should deny the transition when new scope is forbidden")]
    [InlineData("Personal", "Personal", "file-manager.personal-files.update.own")]
    [InlineData("Personal", "Avatar", "file-manager.personal-files.update.all")]
    [InlineData("Avatar", "Personal", "file-manager.avatars.update")]
    public async Task HandleAsync_Should_DenyTransition_When_NewScopeIsForbidden(
        string sourceType, string targetType, string permission)
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile(sourceType, actorId);
        await SaveAsync(file);

        using var response = await SendAsync(
            file.Id.Value,
            new UpdateFileRequest("after.txt", targetType, targetType == "Personal" ? Guid.NewGuid() : null),
            actorId,
            permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var stored = await ReadAsync(file.Id.Value);
        stored.Name.ShouldBe("before.txt");
        stored.Type.ShouldBe(sourceType);
        stored.UserId.ShouldBe(file.UserId?.Value);
    }

    [Theory(DisplayName = "PUT file should preserve all fields when request is invalid or unavailable")]
    [InlineData("id", 400)]
    [InlineData("name", 400)]
    [InlineData("type", 400)]
    [InlineData("owner", 400)]
    [InlineData("folder", 400)]
    [InlineData("missing-owner", 400)]
    [InlineData("unexpected-owner", 400)]
    [InlineData("missing", 404)]
    [InlineData("deleted", 404)]
    [InlineData("deleting", 409)]
    [InlineData("folder-missing", 404)]
    [InlineData("folder-type", 400)]
    [InlineData("folder-owner", 400)]
    [InlineData("folder-forbidden", 404)]
    [InlineData("anonymous", 401)]
    public async Task HandleAsync_Should_PreserveAllFields_When_RequestIsInvalidOrUnavailable(string kind, int code)
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile("Personal", actorId);
        var original = CreateFolder("Personal", actorId);
        var folder = CreateFolder(
            kind == "folder-type" ? "Avatar" : "Personal",
            kind == "folder-type" ? null : kind is "folder-owner" or "folder-forbidden" ? Guid.NewGuid() : actorId);
        file.MoveTo(original);
        if (kind == "deleting")
        {
            file.BeginDeletion();
        }
        await SaveAsync(file, original, folder);
        if (kind == "deleted")
        {
            await using var scope = Fixture.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.Remove(
                await db.Set<File>().SingleAsync(item => item.Id == file.Id, TestContext.Current.CancellationToken));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        var request = new UpdateFileRequest(
            kind == "name" ? "../file.txt" : "after.txt",
            kind == "type" ? "unknown" : kind == "unexpected-owner" ? "Avatar" : "Personal",
            kind == "owner" ? Guid.Empty : kind == "missing-owner" ? null : actorId,
            kind == "folder" ? Guid.Empty : kind == "folder-missing" ? Guid.NewGuid() : folder.Id.Value);

        using var response = await SendAsync(
            kind == "missing" ? Guid.NewGuid() : kind == "id" ? Guid.Empty : file.Id.Value,
            request,
            kind == "anonymous" ? null : actorId,
            kind == "folder-forbidden" ? "file-manager.personal-files.update.own" : AllUpdates);

        ((int)response.StatusCode).ShouldBe(code);
        var stored = await ReadAsync(file.Id.Value);
        stored.Name.ShouldBe("before.txt");
        stored.Type.ShouldBe("Personal");
        stored.UserId.ShouldBe(actorId);
        stored.FolderId.ShouldBe(original.Id.Value);
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
    }

    [Theory(DisplayName = "PUT file should clear optional fields when fields are null or omitted")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ClearOptionalFields_When_FieldsAreNullOrOmitted(bool omitted)
    {
        var actorId = Guid.NewGuid();
        var file = CreateFile("Personal", actorId);
        var folder = CreateFolder("Personal", actorId);
        file.MoveTo(folder);
        await SaveAsync(file, folder);
        object body = omitted
            ? new { Name = "after.txt", Type = "Avatar" }
            : new UpdateFileRequest("after.txt", "Avatar");

        using var response = await SendAsync(file.Id.Value, body, actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await ReadAsync(file.Id.Value);
        stored.Name.ShouldBe("after.txt");
        stored.Type.ShouldBe("Avatar");
        stored.UserId.ShouldBeNull();
        stored.FolderId.ShouldBeNull();
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
        stored.Status.ShouldBe("PendingUpload");
    }

    [Theory(DisplayName = "PUT file should preserve scope consistency when folder update runs concurrently")]
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
        var fileUpdate = SendAsync(
            file.Id.Value,
            new UpdateFileRequest("after.txt", "Personal", actorId, folder.Id.Value),
            actorId,
            AllUpdates);
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
        using var fileResponse = await fileUpdate;

        waiting.ShouldBeTrue();
        var stored = await ReadAsync(file.Id.Value);
        if (fileResponse.StatusCode == HttpStatusCode.NoContent)
        {
            updateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            stored.FolderId.ShouldBe(folder.Id.Value);
        }
        else
        {
            fileResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            stored.FolderId.ShouldBeNull();
        }
        stored.Name.ShouldBe(fileResponse.StatusCode == HttpStatusCode.NoContent ? "after.txt" : "before.txt");
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
            FileName.Create("before.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(ownerId).Value : null).Value;
    }

    private static Folder CreateFolder(string type, Guid? ownerId)
    {
        return Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.FromName(type),
            UserId.Create(ownerId).Value).Value;
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
        return await db.Set<FileReadDbModel>().IgnoreQueryFilters().SingleAsync(
            file => file.Id == id, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, object body, Guid? actorId, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"{Route}/{id}");
        request.Content = JsonContent.Create(body);
        if (actorId is { } value)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, value.ToString());
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
