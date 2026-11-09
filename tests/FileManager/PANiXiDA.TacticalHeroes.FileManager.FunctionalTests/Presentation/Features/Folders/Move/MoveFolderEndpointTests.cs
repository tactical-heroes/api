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
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Move;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Folders.Move;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class MoveFolderEndpointTests(ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/folders";

    [Theory(DisplayName = "PATCH folder move should preserve contents when destination is compatible")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_PreserveContents_When_DestinationIsCompatible(string type)
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var source = CreateFolder(type, userId);
        var target = CreateFolder(type, userId);
        var child = source.CreateChild(FolderName.Create("Child").Value);
        var file = File.Create(FileName.Create("file.txt").Value, source.AllowedFileType, source.UserId).Value;
        file.MoveTo(source);
        await SaveAsync(source, target, child);
        uint targetVersion;
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.Add(file);
            await db.SaveChangesAsync(ct);
            targetVersion = await db.Set<Folder>().Where(folder => folder.Id == target.Id)
                .Select(folder => EF.Property<uint>(folder, "xmin"))
                .SingleAsync(ct);
        }

        using var response = await SendAsync(
            source.Id.Value, new MoveFolderRequest(target.Id.Value), userId.ToString(), Permission(type));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsByteArrayAsync(ct)).ShouldBeEmpty();
        var stored = await ReadAsync(source.Id.Value);
        stored.ParentId.ShouldBe(target.Id.Value);
        stored.Name.ShouldBe(source.Name.Value);
        stored.UserId.ShouldBe(source.UserId?.Value);
        stored.AllowedFileType.ShouldBe(type);
        (await ReadAsync(child.Id.Value)).ParentId.ShouldBe(source.Id.Value);
        await using var verification = Fixture.CreateScope();
        var read = verification.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        (await read.Set<FileReadDbModel>().SingleAsync(ct)).FolderId.ShouldBe(source.Id.Value);
        var write = verification.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var savedTargetVersion = await write.Set<Folder>().Where(folder => folder.Id == target.Id)
            .Select(folder => EF.Property<uint>(folder, "xmin"))
            .SingleAsync(ct);
        savedTargetVersion.ShouldBe(targetVersion);
        using var get = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{source.Id.Value}");
        get.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        get.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            type == "Personal" ? "file-manager.personal-files.read.own" : "file-manager.avatars.read");
        using var detailsResponse = await Fixture.Client.SendAsync(get, ct);
        detailsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await detailsResponse.Content.ReadFromJsonAsync<GetFolderDetailsResponse>(ct);
        details.ShouldNotBeNull();
        details.ParentId.ShouldBe(target.Id.Value);
        details.ParentName.ShouldBe(target.Name.Value);
    }

    [Theory(DisplayName = "PATCH folder move should detach the folder when parent is null or omitted")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_DetachFolder_When_ParentIsNullOrOmitted(bool omitted)
    {
        var userId = Guid.NewGuid();
        var root = CreateFolder("Personal", userId);
        var folder = root.CreateChild(FolderName.Create("Child").Value);
        await SaveAsync(root, folder);
        object body = omitted ? new { } : new MoveFolderRequest(ParentId: null);

        using var response = await SendAsync(
            folder.Id.Value, body, userId.ToString(), Permission("Personal"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await ReadAsync(folder.Id.Value);
        stored.ParentId.ShouldBeNull();
        stored.UserId.ShouldBe(userId);
        stored.Name.ShouldBe("Child");
    }

    [Theory(DisplayName = "PATCH folder move should reject cycles when destination is self or a descendant")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task HandleAsync_Should_RejectCycles_When_DestinationIsSelfOrADescendant(int depth)
    {
        var userId = Guid.NewGuid();
        var source = CreateFolder("Personal", userId);
        List<Folder> chain = [source];
        for (var index = 0; index < depth; index++)
        {
            chain.Add(chain[^1].CreateChild(FolderName.Create($"Level {index}").Value));
        }
        await SaveAsync([.. chain]);

        using var response = await SendAsync(
            source.Id.Value, new MoveFolderRequest(chain[^1].Id.Value), userId.ToString(), Permission("Personal"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        foreach (var folder in chain)
        {
            (await ReadAsync(folder.Id.Value)).ParentId.ShouldBe(folder.ParentId?.Value);
        }
    }

    [Theory(DisplayName = "PATCH folder move should prevent cycles when opposite moves execute concurrently")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_PreventCycles_When_OppositeMovesExecuteConcurrently(string type)
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var first = CreateFolder(type, userId);
        var second = CreateFolder(type, userId);
        await SaveAsync(first, second);
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await scope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>().AcquireAsync(ct);
        var firstMove = SendAsync(
            first.Id.Value, new MoveFolderRequest(second.Id.Value), userId.ToString(), Permission(type));
        var secondMove = SendAsync(
            second.Id.Value, new MoveFolderRequest(first.Id.Value), userId.ToString(), Permission(type));
        bool bothWaiting;

        try
        {
            bothWaiting = await WaitForMovesAsync(db, ct);
        }
        finally
        {
            await transaction.RollbackAsync(ct);
        }
        using var firstResponse = await firstMove;
        using var secondResponse = await secondMove;

        bothWaiting.ShouldBeTrue();
        HttpStatusCode[] statuses = [firstResponse.StatusCode, secondResponse.StatusCode];
        statuses.ShouldBe([HttpStatusCode.NoContent, HttpStatusCode.BadRequest], ignoreOrder: true);
        var firstStored = await ReadAsync(first.Id.Value);
        var secondStored = await ReadAsync(second.Id.Value);
        firstStored.ParentId.ShouldBe(firstResponse.StatusCode == HttpStatusCode.NoContent ? second.Id.Value : null);
        secondStored.ParentId.ShouldBe(secondResponse.StatusCode == HttpStatusCode.NoContent ? first.Id.Value : null);
    }

    [Theory(DisplayName = "PATCH folder move should apply update scopes when type and ownership vary")]
    [InlineData("Personal", true, "file-manager.personal-files.update.own", true)]
    [InlineData("Personal", false, "file-manager.personal-files.update.own", false)]
    [InlineData("Personal", false, "file-manager.personal-files.update.all", true)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", false)]
    [InlineData("Personal", true, "file-manager.avatars.update", false)]
    [InlineData("Personal", true, "", false)]
    [InlineData("Avatar", false, "file-manager.avatars.update", true)]
    [InlineData("Avatar", false, "file-manager.personal-files.update.all", false)]
    public async Task HandleAsync_Should_ApplyUpdateScopes_When_TypeAndOwnershipVary(
        string type, bool own, string permission, bool allowed)
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder(type, own ? userId : Guid.NewGuid());
        var target = CreateFolder(type, folder.UserId?.Value ?? userId);
        await SaveAsync(folder, target);

        using var response = await SendAsync(
            folder.Id.Value, new MoveFolderRequest(target.Id.Value), userId.ToString(), permission);

        response.StatusCode.ShouldBe(allowed ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
        (await ReadAsync(folder.Id.Value)).ParentId.ShouldBe(allowed ? target.Id.Value : null);
    }

    [Theory(DisplayName = "PATCH folder move should reject the destination when type or owner is incompatible")]
    [InlineData("Avatar", "file-manager.personal-files.update.all,file-manager.avatars.update", 400)]
    [InlineData("Personal", "file-manager.personal-files.update.all", 400)]
    [InlineData("Personal", "file-manager.personal-files.update.own", 404)]
    public async Task HandleAsync_Should_RejectDestination_When_TypeOrOwnerIsIncompatible(
        string type, string permission, int code)
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder("Personal", userId);
        var target = CreateFolder(type, Guid.NewGuid());
        await SaveAsync(folder, target);

        using var response = await SendAsync(
            folder.Id.Value, new MoveFolderRequest(target.Id.Value), userId.ToString(), permission);

        ((int)response.StatusCode).ShouldBe(code);
        (await ReadAsync(folder.Id.Value)).ParentId.ShouldBeNull();
    }

    [Theory(DisplayName = "PATCH folder move should reject the request when identifiers or account are unavailable")]
    [InlineData("missing", 404)]
    [InlineData("deleted", 404)]
    [InlineData("parent-missing", 404)]
    [InlineData("parent-deleted", 404)]
    [InlineData("empty", 400)]
    [InlineData("parent-empty", 400)]
    [InlineData("anonymous", 401)]
    public async Task HandleAsync_Should_RejectRequest_When_IdentifiersOrAccountAreUnavailable(string kind, int code)
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder("Personal", userId);
        var parent = CreateFolder("Personal", userId);
        await SaveAsync(folder, parent);
        if (kind is "deleted" or "parent-deleted")
        {
            await using var scope = Fixture.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            var deletedId = kind == "deleted" ? folder.Id : parent.Id;
            var deletedFolder = await db.Set<Folder>()
                .SingleAsync(item => item.Id == deletedId, TestContext.Current.CancellationToken);
            db.Remove(deletedFolder);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        var id = kind == "missing" ? Guid.NewGuid() : kind == "empty" ? Guid.Empty : folder.Id.Value;
        var parentId = kind == "parent-missing"
            ? Guid.NewGuid()
            : kind == "parent-empty" ? Guid.Empty : parent.Id.Value;

        using var response = await SendAsync(
            id,
            new MoveFolderRequest(parentId),
            kind == "anonymous" ? null : userId.ToString(),
            Permission("Personal"));

        ((int)response.StatusCode).ShouldBe(code);
        (await ReadAsync(folder.Id.Value)).ParentId.ShouldBeNull();
    }

    [Fact(DisplayName = "PATCH folder move should accept retries when destination is unchanged")]
    public async Task HandleAsync_Should_AcceptRetries_When_DestinationIsUnchanged()
    {
        var userId = Guid.NewGuid();
        var root = CreateFolder("Personal", userId);
        var folder = root.CreateChild(FolderName.Create("Child").Value);
        await SaveAsync(root, folder);
        var request = new MoveFolderRequest(root.Id.Value);

        using var first = await SendAsync(folder.Id.Value, request, userId.ToString(), Permission("Personal"));
        using var second = await SendAsync(folder.Id.Value, request, userId.ToString(), Permission("Personal"));

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(folder.Id.Value)).ParentId.ShouldBe(root.Id.Value);
    }

    private static string Permission(string type)
    {
        return type == "Personal" ? "file-manager.personal-files.update.own" : "file-manager.avatars.update";
    }

    private static Folder CreateFolder(string type, Guid userId)
    {
        return Folder.Create(
            FolderName.Create("Folder").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(userId).Value : null).Value;
    }

    private static async Task<bool> WaitForMovesAsync(
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

    private async Task<FolderReadDbModel> ReadAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        return await db.Set<FolderReadDbModel>().IgnoreQueryFilters()
            .SingleAsync(folder => folder.Id == id, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, object body, string? subject, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{Route}/{id}/parent");
        request.Content = JsonContent.Create(body);
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
