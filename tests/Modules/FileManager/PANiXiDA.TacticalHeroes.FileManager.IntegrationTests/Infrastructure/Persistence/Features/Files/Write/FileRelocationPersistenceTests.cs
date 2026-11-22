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
    [Theory(DisplayName = "Relocation should preserve its shared identifier when target scope varies")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveChangesAsync_Should_PersistOperationAndMetadata_When_TargetScopeVaries(bool personal)
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
        var attemptId = file.OperationAttemptId;
        var destinationKey = FileStorageKey.Create(id: file.Id, type: file.Type, userId: file.UserId).Value;
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(folder);
        db.Add(file);

        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        var stored = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);

        stored.Status.ShouldBe(FileStatus.Relocating);
        stored.Name.Value.ShouldBe("after.txt");
        stored.Type.ShouldBe(type);
        stored.UserId.ShouldBe(personal ? owner : null);
        stored.FolderId.ShouldBe(folder.Id);
        stored.StorageKey.ShouldBe(sourceKey);
        stored.VersionId!.Value.ShouldBe("active");
        stored.OperationAttemptId.ShouldBe(attemptId);
        FileStorageKey.Create(id: stored.Id, type: stored.Type, userId: stored.UserId)
            .Value
            .ShouldBe(destinationKey);

        stored.CompleteRelocation(attemptId!, FileVersionId.Create("copied").Value).IsSuccess.ShouldBeTrue();
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        var completed = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);

        completed.Status.ShouldBe(FileStatus.Ready);
        completed.Name.Value.ShouldBe("after.txt");
        completed.Type.ShouldBe(type);
        completed.UserId.ShouldBe(personal ? owner : null);
        completed.FolderId.ShouldBe(folder.Id);
        completed.StorageKey.ShouldBe(destinationKey);
        completed.VersionId!.Value.ShouldBe("copied");
        completed.OperationAttemptId.ShouldBeNull();
    }
}
