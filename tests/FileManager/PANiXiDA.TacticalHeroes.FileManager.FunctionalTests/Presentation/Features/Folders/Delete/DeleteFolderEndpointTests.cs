using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.ResultPattern;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

using Wolverine.Tracking;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Folders.Delete;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class DeleteFolderEndpointTests(ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/folders";

    [Theory(DisplayName = "DELETE folder should cascade through nested content when deletion is permitted")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_CascadeThroughNestedContent_When_DeletionIsPermitted(string type)
    {
        var userId = Guid.NewGuid();
        var root = CreateFolder(type, userId);
        var child = root.CreateChild(FolderName.Create("Child").Value);
        var grandchild = child.CreateChild(FolderName.Create("Grandchild").Value);
        var sibling = root.CreateChild(FolderName.Create("Sibling").Value);
        Folder[] folders = [root, child, grandchild, sibling];
        var files = folders.Select((folder, index) => CreateFile(folder, index)).ToArray();
        var other = CreateFolder(type, userId);
        var otherFile = CreateFile(other, 1);
        await SaveAsync([.. folders, other], [.. files, otherFile]);
        await UploadAsync([.. files, otherFile]);
        var permission = type == "Personal" ? "file-manager.personal-files.delete.own" : "file-manager.avatars.delete";

        using var response = await SendAsync(root.Id.Value, userId.ToString(), permission);
        await WaitForCascadeAsync(folders, files);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
        foreach (var folder in folders)
        {
            using var get = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{folder.Id.Value}");
            get.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
            get.Headers.Add(
                TestAuthenticationHandler.PermissionsHeader,
                type == "Personal" ? "file-manager.personal-files.read.own" : "file-manager.avatars.read");
            using var getResponse = await Fixture.Client.SendAsync(get, TestContext.Current.CancellationToken);
            getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var visibleFolders = await db.Set<FolderReadDbModel>().ToListAsync(TestContext.Current.CancellationToken);
        visibleFolders.ShouldHaveSingleItem().Id.ShouldBe(other.Id.Value);
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        foreach (var file in files)
        {
            var metadata = await storage.GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
            metadata.ShouldNotBeNull();
        }
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);
        var ids = files.Select(file => file.Id.Value).ToArray();
        (await db.Set<FileReadDbModel>().IgnoreQueryFilters()
            .Where(file => ids.Contains(file.Id) && file.DeletedAt != null)
            .CountAsync(TestContext.Current.CancellationToken))
            .ShouldBe(files.Length);
        foreach (var file in files)
        {
            var metadata = await storage.GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
            metadata.ShouldBeNull();
        }
        var preserved = await db.Set<FileReadDbModel>()
            .SingleAsync(file => file.Id == otherFile.Id.Value, TestContext.Current.CancellationToken);
        preserved.Status.ShouldBe("Ready");
        using var repeated = await SendAsync(root.Id.Value, userId.ToString(), permission);
        repeated.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory(
        DisplayName = "Reconciliation should resume cascading deletion when entries commit after the cascade drained")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_ResumeCascadingDeletion_When_EntriesCommitAfterTheCascadeDrained(string type)
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var root = CreateFolder(type, userId);
        var otherRoot = CreateFolder(type, userId);
        await SaveAsync([root, otherRoot], []);
        await using var creatingScope = Fixture.CreateScope();
        var repository = creatingScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var staleParent = await repository.GetByIdAsync(root.Id, ct);
        var child = staleParent!.CreateChild(FolderName.Create("Late child").Value);
        var grandchild = child.CreateChild(FolderName.Create("Late grandchild").Value);
        var files = new[]
        {
            CreateFile(staleParent, 1), CreateFile(child, 0), CreateFile(grandchild, 1), CreateFile(otherRoot, 1)
        };
        var permission = type == "Personal" ? "file-manager.personal-files.delete.own" : "file-manager.avatars.delete";
        using var response = await SendAsync(
            root.Id.Value, userId.ToString(), permission);
        using var otherResponse = await SendAsync(otherRoot.Id.Value, userId.ToString(), permission);
        await WaitForCascadeAsync([root, otherRoot], []);
        await creatingScope.ServiceProvider.TrackActivity()
            .PublishMessageAndWaitAsync(new FolderDeleted(root.Id.Value));
        await creatingScope.ServiceProvider.TrackActivity()
            .PublishMessageAndWaitAsync(new FolderDeleted(otherRoot.Id.Value));
        var creatingDb = creatingScope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var version = await creatingDb.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(folder => folder.Id == root.Id)
            .Select(folder => EF.Property<uint>(folder, "xmin"))
            .SingleAsync(ct);
        creatingDb.AddRange(child, grandchild);
        creatingDb.AddRange(files);
        await creatingDb.SaveChangesAsync(ct);
        var unchangedVersion = await creatingDb.Set<Folder>()
            .IgnoreQueryFilters()
            .Where(folder => folder.Id == root.Id)
            .Select(folder => EF.Property<uint>(folder, "xmin"))
            .SingleAsync(ct);
        await UploadAsync(files);
        await using var firstScope = Fixture.CreateScope();
        var firstMediator = firstScope.ServiceProvider.GetRequiredService<IMediator>();
        Result result = null!;

        var session = await firstScope.ServiceProvider.ExecuteAndWaitAsync(async () =>
        {
            result = await firstMediator.SendAsync(new ReconcileFileUploadsCommand(), ct);
        });
        await using var secondScope = Fixture.CreateScope();
        var secondMediator = secondScope.ServiceProvider.GetRequiredService<IMediator>();
        await secondMediator.SendAsync(new ReconcileFileUploadsCommand(), ct);

        result.IsSuccess.ShouldBeTrue(string.Join("; ", result.Errors.Select(error => error.Message)));
        session.FindEnvelopesWithMessageType<FolderDeleted>().ShouldNotBeEmpty();
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        otherResponse.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        unchangedVersion.ShouldBe(version);
        var db = secondScope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        (await db.Set<FolderReadDbModel>().AnyAsync(ct)).ShouldBeFalse();
        (await db.Set<FileReadDbModel>().AnyAsync(ct)).ShouldBeFalse();
        (await db.Set<FolderReadDbModel>().IgnoreQueryFilters().CountAsync(ct)).ShouldBe(4);
        (await db.Set<FileReadDbModel>().IgnoreQueryFilters().CountAsync(ct)).ShouldBe(4);
        var storage = secondScope.ServiceProvider.GetRequiredService<IFileStorage>();
        foreach (var file in files)
        {
            (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldBeNull();
        }
    }

    [Fact(DisplayName = "DELETE folder should drain bounded batches when files and child counts exceed the limit")]
    public async Task HandleAsync_Should_DrainBoundedBatches_When_FilesAndChildCountsExceedTheLimit()
    {
        var userId = Guid.NewGuid();
        var root = CreateFolder("Personal", userId);
        var children = Enumerable.Range(0, 103)
            .Select(index => root.CreateChild(FolderName.Create($"Child {index}").Value))
            .ToArray();
        var files = Enumerable.Range(0, 107).Select(index => CreateFile(root, index * 3)).ToArray();
        await SaveAsync([root, .. children], files);

        using var response = await SendAsync(
            root.Id.Value, userId.ToString(), "file-manager.personal-files.delete.own");
        await WaitForCascadeAsync([root, .. children], files);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        (await db.Set<FolderReadDbModel>().IgnoreQueryFilters().CountAsync(TestContext.Current.CancellationToken))
            .ShouldBe(104);
        (await db.Set<FileReadDbModel>().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(107);
    }

    [Fact(DisplayName = "Folder deletion should be idempotent when events are delivered repeatedly")]
    public async Task PublishAsync_Should_BeIdempotent_When_EventsAreDeliveredRepeatedly()
    {
        var userId = Guid.NewGuid();
        var root = CreateFolder("Personal", userId);
        var child = root.CreateChild(FolderName.Create("Child").Value);
        var file = CreateFile(child, 1);
        await SaveAsync([root, child], [file]);
        using var response = await SendAsync(
            root.Id.Value, userId.ToString(), "file-manager.personal-files.delete.own");
        await WaitForCascadeAsync([root, child], [file]);
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var before = await db.Set<File>().Where(item => item.Id == file.Id)
            .Select(item => EF.Property<uint>(item, "xmin"))
            .SingleAsync(TestContext.Current.CancellationToken);
        var domainEvent = new FolderDeleted(root.Id.Value);

        await scope.ServiceProvider.TrackActivity().PublishMessageAndWaitAsync(domainEvent);
        await scope.ServiceProvider.TrackActivity().PublishMessageAndWaitAsync(domainEvent);
        await scope.ServiceProvider.TrackActivity().PublishMessageAndWaitAsync(new FolderDeleted(root.Id.Value));

        var after = await db.Set<File>().Where(item => item.Id == file.Id)
            .Select(item => EF.Property<uint>(item, "xmin"))
            .SingleAsync(TestContext.Current.CancellationToken);
        after.ShouldBe(before);
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
    }

    [Theory(DisplayName = "DELETE folder should respect permission scopes when type and ownership vary")]
    [InlineData("Personal", true, "file-manager.personal-files.delete.own", true)]
    [InlineData("Personal", false, "file-manager.personal-files.delete.own", false)]
    [InlineData("Personal", false, "file-manager.personal-files.delete.all", true)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", false)]
    [InlineData("Personal", true, "file-manager.avatars.delete", false)]
    [InlineData("Personal", true, "", false)]
    [InlineData("Avatar", false, "file-manager.avatars.delete", true)]
    [InlineData("Avatar", false, "file-manager.personal-files.delete.all", false)]
    [InlineData("Avatar", false, "file-manager.avatars.read", false)]
    public async Task HandleAsync_Should_RespectPermissionScopes_When_TypeAndOwnershipVary(
        string type, bool own, string permission, bool allowed)
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder(type, own ? userId : Guid.NewGuid());
        await SaveAsync([folder], []);

        using var response = await SendAsync(folder.Id.Value, userId.ToString(), permission);

        response.StatusCode.ShouldBe(allowed ? HttpStatusCode.Accepted : HttpStatusCode.NotFound);
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var stored = await db.Set<FolderReadDbModel>().IgnoreQueryFilters()
            .SingleAsync(TestContext.Current.CancellationToken);
        (stored.DeletedAt is not null).ShouldBe(allowed);
    }

    [Theory(DisplayName = "DELETE folder should reject the request when identifier or authentication is unavailable")]
    [InlineData("missing", 404)]
    [InlineData("deleted", 404)]
    [InlineData("empty", 400)]
    [InlineData("anonymous", 401)]
    public async Task HandleAsync_Should_RejectRequest_When_IdentifierOrAuthenticationIsUnavailable(
        string kind, int code)
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder("Personal", userId);
        await SaveAsync([folder], []);
        if (kind == "deleted")
        {
            using var initial = await SendAsync(
                folder.Id.Value, userId.ToString(), "file-manager.personal-files.delete.own");
            initial.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        }

        using var response = await SendAsync(
            kind == "missing" ? Guid.NewGuid() : kind == "empty" ? Guid.Empty : folder.Id.Value,
            kind == "anonymous" ? null : userId.ToString(),
            "file-manager.personal-files.delete.own");

        ((int)response.StatusCode).ShouldBe(code);
    }

    private static Folder CreateFolder(string type, Guid userId)
    {
        return Folder.Create(
            FolderName.Create("Root").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(userId).Value : null).Value;
    }

    private static File CreateFile(Folder folder, int index)
    {
        var file = File.Create(FileName.Create($"file-{index}.txt").Value, folder.AllowedFileType, folder.UserId).Value;
        file.MoveTo(folder);
        if (index % 3 != 0)
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(1).Value);
        }
        if (index % 3 == 2)
        {
            file.BeginDeletion();
        }
        return file;
    }

    private async Task SaveAsync(IReadOnlyList<Folder> folders, IReadOnlyList<File> files)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.AddRange(folders);
        db.AddRange(files);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task UploadAsync(IReadOnlyList<File> files)
    {
        await using var scope = Fixture.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        foreach (var file in files)
        {
            await using var content = new MemoryStream([1]);
            await storage.UploadAsync(
                file.StorageKey.Value, content, "text/plain", TestContext.Current.CancellationToken);
        }
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, string? subject, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Route}/{id}");
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task WaitForCascadeAsync(IReadOnlyList<Folder> folders, IReadOnlyList<File> files)
    {
        var folderIds = folders.Select(folder => folder.Id.Value).ToArray();
        var fileIds = files.Select(file => file.Id.Value).ToArray();
        var deadline = DateTimeOffset.UtcNow.AddSeconds(45);
        while (DateTimeOffset.UtcNow < deadline)
        {
            await using var scope = Fixture.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
            var deleted = await db.Set<FolderReadDbModel>().IgnoreQueryFilters()
                .CountAsync(
                    folder => folderIds.Contains(folder.Id) && folder.DeletedAt != null,
                    TestContext.Current.CancellationToken);
            var deleting = await db.Set<FileReadDbModel>()
                .CountAsync(
                    file => fileIds.Contains(file.Id) && file.Status == "Deleting",
                    TestContext.Current.CancellationToken);
            if (deleted == folderIds.Length && deleting == fileIds.Length)
            {
                return;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        }
        throw new TimeoutException("Folder deletion did not finish its cascade.");
    }
}
