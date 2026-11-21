using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Files.Write;

public sealed class FileRelocationPersistenceTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Relocation fields should survive reload and cancellation when target scope varies")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveChangesAsync_Should_PersistAndClearRelocationFields_When_TargetScopeVaries(bool personal)
    {
        var ct = TestContext.Current.CancellationToken;
        var owner = UserId.Create(Guid.NewGuid()).Value;
        var type = personal ? FileType.Personal : FileType.Avatar;
        var folder = Folder.Create(
            FolderName.Create("Target").Value,
            type,
            personal ? owner : null)
            .Value;
        var file = File.Create(
            FileName.Create("before.txt").Value,
            personal ? FileType.Avatar : FileType.Personal,
            personal ? null : owner)
            .Value;
        file.CompleteUpload(
            FileContentType.Create("text/plain").Value,
            FileSize.Create(4).Value,
            FileVersionId.Create("active").Value)
            .IsSuccess
            .ShouldBeTrue();
        var sourceKey = file.StorageKey;
        file.Update(FileName.Create("after.txt").Value, type, personal ? owner : null, folder)
            .IsSuccess
            .ShouldBeTrue();
        var attemptId = file.RelocationAttemptId;
        var destinationKey = file.RelocationStorageKey;
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(folder);
        db.Add(file);

        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        var stored = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);

        stored.Status.ShouldBe(FileStatus.Relocating);
        stored.Name.Value.ShouldBe("before.txt");
        stored.StorageKey.ShouldBe(sourceKey);
        stored.VersionId!.Value.ShouldBe("active");
        stored.RelocationAttemptId.ShouldBe(attemptId);
        stored.RelocationName!.Value.ShouldBe("after.txt");
        stored.RelocationType.ShouldBe(type);
        stored.RelocationUserId.ShouldBe(personal ? owner : null);
        stored.RelocationFolderId.ShouldBe(folder.Id);
        stored.RelocationStorageKey.ShouldBe(destinationKey);

        stored.CancelRelocation(attemptId!).IsSuccess.ShouldBeTrue();
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        var cancelled = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);

        cancelled.Status.ShouldBe(FileStatus.Ready);
        cancelled.Name.Value.ShouldBe("before.txt");
        cancelled.StorageKey.ShouldBe(sourceKey);
        cancelled.VersionId!.Value.ShouldBe("active");
        cancelled.RelocationAttemptId.ShouldBeNull();
        cancelled.RelocationName.ShouldBeNull();
        cancelled.RelocationType.ShouldBeNull();
        cancelled.RelocationUserId.ShouldBeNull();
        cancelled.RelocationFolderId.ShouldBeNull();
        cancelled.RelocationStorageKey.ShouldBeNull();
    }
}
