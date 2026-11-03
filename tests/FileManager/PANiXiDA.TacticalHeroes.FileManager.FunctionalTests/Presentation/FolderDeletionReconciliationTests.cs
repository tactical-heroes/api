using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.ReconcileDeletions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

using Wolverine.Runtime;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation;

public sealed class FolderDeletionReconciliationTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Folder reconciliation should clean up late contents when recurring schedule fires")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task ReconcileAsync_Should_CleanUpLateContents_When_RecurringScheduleFires(string type)
    {
        var ct = TestContext.Current.CancellationToken;
        var root = Folder.Create(
            FolderName.Create("Deleted parent").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(Guid.NewGuid()).Value : null).Value;
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.Add(root);
            await db.SaveChangesAsync(ct);
            db.Remove(root);
            await db.SaveChangesAsync(ct);
        }
        var child = root.CreateChild(FolderName.Create("Late child").Value);
        var files = new[] { CreateFile(root), CreateFile(child) };
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.Add(child);
            db.AddRange(files);
            var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
            foreach (var file in files)
            {
                await using var content = new MemoryStream([1, 2, 3, 4]);
                await storage.UploadAsync(file.StorageKey.Value, content, "text/plain", ct);
            }
            await db.SaveChangesAsync(ct);
        }

        await WaitForDeletionAsync(child.Id, files);

        await using var verificationScope = Fixture.CreateScope();
        var read = verificationScope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        (await read.Set<FolderReadDbModel>().AnyAsync(ct)).ShouldBeFalse();
        (await read.Set<FileReadDbModel>().AnyAsync(ct)).ShouldBeFalse();
        (await read.Set<FolderReadDbModel>().IgnoreQueryFilters().CountAsync(ct)).ShouldBe(2);
        (await read.Set<FileReadDbModel>().IgnoreQueryFilters().CountAsync(ct)).ShouldBe(2);
        var fileStorage = verificationScope.ServiceProvider.GetRequiredService<IFileStorage>();
        foreach (var file in files)
        {
            (await fileStorage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldBeNull();
        }
    }

    [Fact(DisplayName = "Folder reconciliation schedule should target the command when host is configured")]
    public void Scheduling_Should_TargetCommand_When_HostIsConfigured()
    {
        using var scope = Fixture.CreateScope();
        var runtime = scope.ServiceProvider.GetRequiredService<IWolverineRuntime>();

        var schedule = runtime.Options.Schedules.FindByName("file-manager-reconcile-folder-deletions");

        schedule.ShouldNotBeNull();
        schedule.MessageType.ShouldBe(typeof(ReconcileFolderDeletionsCommand));
        schedule.Schedule.Expression.ShouldBe("*/5 * * * * *");
        schedule.Schedule.TimeZone.Id.ShouldBe("UTC");
    }

    private static File CreateFile(Folder folder)
    {
        var file = File.Create(FileName.Create("file.txt").Value, folder.AllowedFileType, folder.UserId).Value;
        file.MoveTo(folder);
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);

        return file;
    }

    private async Task WaitForDeletionAsync(FolderId childId, IReadOnlyList<File> files)
    {
        var ct = TestContext.Current.CancellationToken;
        var fileIds = files.Select(file => file.Id.Value).ToArray();
        var deadline = TimeProvider.System.GetUtcNow().AddSeconds(45);
        while (TimeProvider.System.GetUtcNow() < deadline)
        {
            await using var scope = Fixture.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
            var folderDeleted = await db.Set<FolderReadDbModel>().IgnoreQueryFilters()
                .AnyAsync(folder => folder.Id == childId.Value && folder.DeletedAt != null, ct);
            var filesDeleted = await db.Set<FileReadDbModel>().IgnoreQueryFilters()
                .CountAsync(file => fileIds.Contains(file.Id) && file.DeletedAt != null, ct);
            if (folderDeleted && filesDeleted == files.Count)
            {
                return;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
        }
        throw new TimeoutException("The recurring commands did not complete folder content deletion.");
    }
}
