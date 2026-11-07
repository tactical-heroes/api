using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Update;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Folders.Update;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class UpdateFolderEndpointTests(ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/folders";
    private const string AllUpdates = "file-manager.personal-files.update.all,file-manager.avatars.update";

    [Theory(DisplayName = "PUT folder should replace all fields when folder is empty")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_ReplaceAllFields_When_FolderIsEmpty(string type)
    {
        var actorId = Guid.NewGuid();
        var ownerId = type == "Personal" ? Guid.NewGuid() : (Guid?)null;
        var folder = CreateFolder("Personal", actorId);
        var parent = CreateFolder(type, ownerId);
        await SaveAsync(folder, parent);
        var version = await VersionAsync(parent.Id);

        using var response = await SendAsync(
            folder.Id.Value, new UpdateFolderRequest(" After ", type, ownerId, parent.Id.Value), actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
        var stored = await ReadAsync(folder.Id.Value);
        stored.Name.ShouldBe("After");
        stored.AllowedFileType.ShouldBe(type);
        stored.UserId.ShouldBe(ownerId);
        stored.ParentId.ShouldBe(parent.Id.Value);
        (await VersionAsync(parent.Id)).ShouldBe(version);
    }

    [Theory(DisplayName = "PUT folder should preserve contents when only name and parent change")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_PreserveContents_When_OnlyNameAndParentChange(string type)
    {
        var actorId = Guid.NewGuid();
        var ownerId = type == "Personal" ? actorId : (Guid?)null;
        var folder = CreateFolder(type, ownerId);
        var parent = CreateFolder(type, ownerId);
        var child = folder.CreateChild(FolderName.Create("Child").Value);
        var file = CreateFile(folder);
        await SaveAsync(folder, parent, child);
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.Add(file);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var response = await SendAsync(
            folder.Id.Value, new UpdateFolderRequest("After", type, ownerId, parent.Id.Value), actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await ReadAsync(folder.Id.Value);
        stored.Name.ShouldBe("After");
        stored.ParentId.ShouldBe(parent.Id.Value);
        stored.AllowedFileType.ShouldBe(type);
        stored.UserId.ShouldBe(ownerId);
        (await ReadAsync(child.Id.Value)).ParentId.ShouldBe(folder.Id.Value);
        await using var verification = Fixture.CreateScope();
        var read = verification.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var storedFile = await read.Set<FileReadDbModel>().SingleAsync(TestContext.Current.CancellationToken);
        storedFile.FolderId.ShouldBe(folder.Id.Value);
        storedFile.StorageKey.ShouldBe(file.StorageKey.Value);
        storedFile.UserId.ShouldBe(ownerId);
    }

    [Theory(DisplayName = "PUT folder should detach while renaming when parent is null or omitted")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_DetachWhileRenaming_When_ParentIsNullOrOmitted(bool omitted)
    {
        var actorId = Guid.NewGuid();
        var parent = CreateFolder("Personal", actorId);
        var folder = parent.CreateChild(FolderName.Create("Child").Value);
        await SaveAsync(parent, folder);
        object body = omitted
            ? new { Name = "After", AllowedFileType = "Personal", UserId = actorId }
            : new UpdateFolderRequest("After", "Personal", actorId, ParentId: null);

        using var response = await SendAsync(folder.Id.Value, body, actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await ReadAsync(folder.Id.Value);
        stored.Name.ShouldBe("After");
        stored.ParentId.ShouldBeNull();
        stored.UserId.ShouldBe(actorId);
    }

    [Theory(DisplayName = "PUT folder should require metadata when body omits mandatory fields")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RequireMetadata_When_BodyOmitsMandatoryFields(bool missingName)
    {
        var actorId = Guid.NewGuid();
        var folder = CreateFolder("Personal", actorId);
        await SaveAsync(folder);
        object body = missingName
            ? new { AllowedFileType = "Personal", UserId = actorId }
            : new { Name = "After", UserId = actorId };

        using var response = await SendAsync(folder.Id.Value, body, actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(folder.Id.Value)).Name.ShouldBe("Before");
    }

    [Theory(DisplayName = "PUT folder should preserve all fields when scope changes with active contents")]
    [InlineData("child", false)]
    [InlineData("child", true)]
    [InlineData("pending", false)]
    [InlineData("pending", true)]
    [InlineData("deleting", false)]
    [InlineData("deleting", true)]
    public async Task HandleAsync_Should_PreserveAllFields_When_ScopeChangesWithActiveContents(
        string contents, bool changeType)
    {
        var actorId = Guid.NewGuid();
        var root = CreateFolder("Personal", actorId);
        var folder = root.CreateChild(FolderName.Create("Before").Value);
        var type = changeType ? "Avatar" : "Personal";
        var ownerId = changeType ? null : (Guid?)Guid.NewGuid();
        var target = CreateFolder(type, ownerId);
        await SaveAsync(root, folder, target);
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            if (contents == "child")
            {
                db.Add(folder.CreateChild(FolderName.Create("Child").Value));
            }
            else
            {
                var file = CreateFile(folder);
                if (contents == "deleting")
                {
                    file.BeginDeletion();
                }
                db.Add(file);
            }
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var response = await SendAsync(
            folder.Id.Value, new UpdateFolderRequest("After", type, ownerId, target.Id.Value), actorId, AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var stored = await ReadAsync(folder.Id.Value);
        stored.Name.ShouldBe("Before");
        stored.AllowedFileType.ShouldBe("Personal");
        stored.UserId.ShouldBe(actorId);
        stored.ParentId.ShouldBe(root.Id.Value);
    }

    [Theory(DisplayName = "PUT folder should apply update scopes when type and ownership vary")]
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
        var folder = CreateFolder(type, type == "Personal" ? own ? actorId : Guid.NewGuid() : null);
        await SaveAsync(folder);

        using var response = await SendAsync(
            folder.Id.Value, new UpdateFolderRequest("After", type, folder.UserId?.Value), actorId, permission);

        ((int)response.StatusCode).ShouldBe(code);
        (await ReadAsync(folder.Id.Value)).Name.ShouldBe(code == 204 ? "After" : "Before");
    }

    [Theory(DisplayName = "PUT folder should deny the transition when new scope is forbidden")]
    [InlineData("Personal", "Personal", "file-manager.personal-files.update.own")]
    [InlineData("Personal", "Avatar", "file-manager.personal-files.update.all")]
    [InlineData("Avatar", "Personal", "file-manager.avatars.update")]
    public async Task HandleAsync_Should_DenyTransition_When_NewScopeIsForbidden(
        string sourceType, string targetType, string permission)
    {
        var actorId = Guid.NewGuid();
        var folder = CreateFolder(sourceType, sourceType == "Personal" ? actorId : null);
        await SaveAsync(folder);

        using var response = await SendAsync(
            folder.Id.Value,
            new UpdateFolderRequest("After", targetType, targetType == "Personal" ? Guid.NewGuid() : null),
            actorId,
            permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var stored = await ReadAsync(folder.Id.Value);
        stored.Name.ShouldBe("Before");
        stored.AllowedFileType.ShouldBe(sourceType);
        stored.UserId.ShouldBe(folder.UserId?.Value);
    }

    [Theory(DisplayName = "PUT folder should preserve all fields when request is invalid or unavailable")]
    [InlineData("name", 400)]
    [InlineData("type", 400)]
    [InlineData("owner", 400)]
    [InlineData("parent", 400)]
    [InlineData("missing-owner", 400)]
    [InlineData("missing", 404)]
    [InlineData("deleted", 404)]
    [InlineData("parent-missing", 404)]
    [InlineData("anonymous", 401)]
    public async Task HandleAsync_Should_PreserveAllFields_When_RequestIsInvalidOrUnavailable(string kind, int code)
    {
        var actorId = Guid.NewGuid();
        var folder = CreateFolder("Personal", actorId);
        await SaveAsync(folder);
        if (kind == "deleted")
        {
            await using var scope = Fixture.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.Remove(await db.Set<Folder>().SingleAsync(
                item => item.Id == folder.Id, TestContext.Current.CancellationToken));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        var request = new UpdateFolderRequest(
            kind == "name" ? "../folder" : "After",
            kind == "type" ? "unknown" : "Personal",
            kind == "owner" ? Guid.Empty : kind == "missing-owner" ? null : actorId,
            kind == "parent" ? Guid.Empty : kind == "parent-missing" ? Guid.NewGuid() : null);

        using var response = await SendAsync(
            kind == "missing" ? Guid.NewGuid() : folder.Id.Value,
            request,
            kind == "anonymous" ? null : actorId,
            AllUpdates);

        ((int)response.StatusCode).ShouldBe(code);
        var stored = await ReadAsync(folder.Id.Value);
        stored.Name.ShouldBe("Before");
        stored.AllowedFileType.ShouldBe("Personal");
        stored.UserId.ShouldBe(actorId);
        stored.ParentId.ShouldBeNull();
    }

    [Theory(DisplayName = "PUT folder should reject cycles atomically when destination is self or a descendant")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task HandleAsync_Should_RejectCyclesAtomically_When_DestinationIsSelfOrADescendant(int depth)
    {
        var actorId = Guid.NewGuid();
        var folder = CreateFolder("Personal", actorId);
        List<Folder> chain = [folder];
        for (var index = 0; index < depth; index++)
        {
            chain.Add(chain[^1].CreateChild(FolderName.Create($"Level {index}").Value));
        }
        await SaveAsync([.. chain]);

        using var response = await SendAsync(
            folder.Id.Value,
            new UpdateFolderRequest("After", "Personal", actorId, chain[^1].Id.Value),
            actorId,
            AllUpdates);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(folder.Id.Value)).Name.ShouldBe("Before");
        (await ReadAsync(folder.Id.Value)).ParentId.ShouldBeNull();
    }

    [Theory(DisplayName = "PUT folder should prevent cycles when another update or move runs concurrently")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreventCycles_When_AnotherUpdateOrMoveRunsConcurrently(bool patch)
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        var first = CreateFolder("Avatar", ownerId: null);
        var second = CreateFolder("Avatar", ownerId: null);
        await SaveAsync(first, second);
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await scope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>().AcquireAsync(ct);
        var firstRequest = SendAsync(
            first.Id.Value, new UpdateFolderRequest("After", "Avatar", ParentId: second.Id.Value), actorId, AllUpdates);
        var secondRequest = patch
            ? SendMoveAsync(second.Id.Value, first.Id.Value, actorId)
            : SendAsync(
                second.Id.Value,
                new UpdateFolderRequest("After", "Avatar", ParentId: first.Id.Value),
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
        using var firstResponse = await firstRequest;
        using var secondResponse = await secondRequest;

        waiting.ShouldBeTrue();
        HttpStatusCode[] statuses = [firstResponse.StatusCode, secondResponse.StatusCode];
        statuses.ShouldBe([HttpStatusCode.NoContent, HttpStatusCode.BadRequest], ignoreOrder: true);
        (await ReadAsync(first.Id.Value)).Name.ShouldBe(
            firstResponse.StatusCode == HttpStatusCode.NoContent ? "After" : "Before");
        var savedFirst = await ReadAsync(first.Id.Value);
        var savedSecond = await ReadAsync(second.Id.Value);
        savedFirst.ParentId.ShouldBe(firstResponse.StatusCode == HttpStatusCode.NoContent ? second.Id.Value : null);
        savedSecond.ParentId.ShouldBe(secondResponse.StatusCode == HttpStatusCode.NoContent ? first.Id.Value : null);
    }

    [Theory(DisplayName = "PUT folder should preserve scope consistency when child creation runs concurrently")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreserveScopeConsistency_When_ChildCreationRunsConcurrently(bool file)
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var folder = CreateFolder("Personal", actorId);
        await SaveAsync(folder);
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await scope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>().AcquireAsync(ct);
        var update = SendAsync(
            folder.Id.Value, new UpdateFolderRequest("After", "Personal", newOwnerId), actorId, AllUpdates);
        var creation = file
            ? SendFileAsync(folder.Id.Value, actorId)
            : SendChildAsync(folder.Id.Value, actorId);
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
        using var creationResponse = await creation;

        waiting.ShouldBeTrue();
        var stored = await ReadAsync(folder.Id.Value);
        if (updateResponse.StatusCode == HttpStatusCode.NoContent)
        {
            creationResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            stored.UserId.ShouldBe(newOwnerId);
            stored.Name.ShouldBe("After");
        }
        else
        {
            updateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            creationResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
            stored.UserId.ShouldBe(actorId);
            stored.Name.ShouldBe("Before");
        }
        await using var verification = Fixture.CreateScope();
        var read = verification.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var children = await read.Set<FolderReadDbModel>()
            .Where(item => item.ParentId == folder.Id.Value)
            .ToListAsync(ct);
        children.ShouldAllBe(item => item.UserId == stored.UserId && item.AllowedFileType == stored.AllowedFileType);
        var files = await read.Set<FileReadDbModel>().Where(item => item.FolderId == folder.Id.Value).ToListAsync(ct);
        files.ShouldAllBe(item => item.UserId == stored.UserId && item.Type == stored.AllowedFileType);
    }

    private static Folder CreateFolder(string type, Guid? ownerId)
    {
        return Folder.Create(
            FolderName.Create("Before").Value, FileType.FromName(type), UserId.Create(ownerId).Value).Value;
    }

    private static File CreateFile(Folder folder)
    {
        var file = File.Create(FileName.Create("file.txt").Value, folder.AllowedFileType, folder.UserId).Value;
        file.MoveTo(folder);
        return file;
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

    private async Task SaveAsync(params Folder[] folders)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.AddRange(folders);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<uint> VersionAsync(FolderId id)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        return await db.Set<Folder>().Where(folder => folder.Id == id)
            .Select(folder => EF.Property<uint>(folder, "xmin"))
            .SingleAsync(TestContext.Current.CancellationToken);
    }

    private async Task<FolderReadDbModel> ReadAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        return await db.Set<FolderReadDbModel>().IgnoreQueryFilters()
            .SingleAsync(folder => folder.Id == id, TestContext.Current.CancellationToken);
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

    private async Task<HttpResponseMessage> SendMoveAsync(Guid id, Guid parentId, Guid actorId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{Route}/{id}/parent");
        request.Content = JsonContent.Create(new { ParentId = parentId });
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, AllUpdates);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> SendChildAsync(Guid parentId, Guid actorId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Route}/personal");
        request.Content = JsonContent.Create(new { Name = "Child", ParentId = parentId });
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, "file-manager.personal-files.create.own");
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> SendFileAsync(Guid folderId, Guid actorId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/files/personal");
        using var form = new MultipartFormDataContent();
        using var content = new ByteArrayContent([1]);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        form.Add(content, "File", "file.txt");
        using var folder = new StringContent(folderId.ToString());
        form.Add(folder, "FolderId");
        request.Content = form;
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, "file-manager.personal-files.create.own");
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
