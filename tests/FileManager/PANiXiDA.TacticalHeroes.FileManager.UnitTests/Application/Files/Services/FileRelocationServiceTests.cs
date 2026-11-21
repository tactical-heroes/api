using PANiXiDA.Core.Application.Messaging.Scheduling;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Constants;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Services;

public sealed class FileRelocationServiceTests
{
    [Theory(DisplayName = "Relocation should retain active content until validated copy when recovery paths vary")]
    [InlineData("copy")]
    [InlineData("latest")]
    [InlineData("historical")]
    [InlineData("missing-version")]
    [InlineData("wrong-version")]
    [InlineData("wrong-metadata")]
    [InlineData("folder-deleted")]
    [InlineData("folder-changed")]
    public async Task RelocateAsync_Should_RetainActiveContentUntilValidatedCopy_When_RecoveryPathsVary(string kind)
    {
        var ct = TestContext.Current.CancellationToken;
        var files = Substitute.For<IFilesRepository>();
        var folders = Substitute.For<IFoldersRepository>();
        var hierarchyLock = Substitute.For<IFolderHierarchyLock>();
        var storage = Substitute.For<IFileStorage>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var scheduler = Substitute.For<IScheduler>();
        unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), ct)
            .Returns(call => call.ArgAt<Func<CancellationToken, Task>>(0)(ct));
        var file = File.Create(FileName.Create("old.txt").Value, FileType.Avatar, null).Value;
        file.CompleteUpload(
            FileContentType.Create("text/plain").Value,
            FileSize.Create(4).Value,
            FileVersionId.Create("source").Value);
        var owner = UserId.Create(Guid.NewGuid()).Value;
        var folder = Folder.Create(FolderName.Create("Target").Value, FileType.Personal, owner).Value;
        file.Update(FileName.Create("new.txt").Value, FileType.Personal, owner, folder).IsSuccess.ShouldBeTrue();
        var relocation = file.Relocation!;
        var sourceKey = file.StorageKey;
        folders.GetUntrackedByIdAsync(folder.Id, ct).Returns(kind == "folder-deleted" ? null : folder);
        if (kind == "folder-changed")
        {
            folders.GetUntrackedByIdAsync(folder.Id, ct).Returns(Folder.Create(
                FolderName.Create("Other").Value,
                FileType.Avatar,
                null)
                .Value);
        }
        var metadata = new FileMetadata(
            "text/plain",
            4,
            new Dictionary<string, string>
            {
                [FileMetadataNames.RelocationAttemptId] = relocation.AttemptId.Value.ToString()
            },
            "destination");
        var current = new FileMetadata("text/plain", 4, new Dictionary<string, string>(), "unrelated");
        storage.GetMetadataAsync(relocation.StorageKey.Value, ct).Returns(kind == "latest" ? metadata : current);
        FileVersion[] versions = kind == "historical"
            ? [
                new FileVersion("unrelated", DateTimeOffset.UnixEpoch),
                new FileVersion("destination", DateTimeOffset.UnixEpoch)
            ]
            : [];
        storage.GetVersionsAsync(relocation.StorageKey.Value, ct).Returns(versions.ToAsyncEnumerable());
        storage.GetMetadataAsync(relocation.StorageKey.Value, "destination", ct)
            .Returns(kind == "wrong-metadata" ? metadata with { Size = 9 }
                : kind == "wrong-version" ? metadata with { VersionId = "other" } : metadata);
        storage.CopyAsync(
            sourceKey.Value,
            "source",
            relocation.StorageKey.Value,
            "text/plain",
            4,
            Arg.Any<IReadOnlyDictionary<string, string>>(),
            ct)
            .Returns(call =>
            {
                file.Status.ShouldBe(FileStatus.Relocating);
                file.StorageKey.ShouldBe(sourceKey);
                file.VersionId!.Value.ShouldBe("source");
                return new FileCopyResult(kind == "missing-version" ? null : "destination");
            });
        var service = new FileRelocationService(files, folders, hierarchyLock, storage, unitOfWork, scheduler);

        var result = await service.RelocateAsync(file, ct);

        var unusable = kind is "missing-version" or "wrong-version" or "wrong-metadata";
        var cancelled = kind is "folder-deleted" or "folder-changed";
        result.IsSuccess.ShouldBe(!unusable && !cancelled);
        file.Status.ShouldBe(unusable ? FileStatus.Relocating : FileStatus.Ready);
        file.StorageKey.ShouldBe(unusable || cancelled ? sourceKey : relocation.StorageKey);
        file.VersionId!.Value.ShouldBe(unusable || cancelled ? "source" : "destination");
        if (!unusable)
        {
            file.Relocation.ShouldBeNull();
            files.Received(1).Update(file);
            await scheduler.Received(1).ScheduleAsync(
                Arg.Is<CleanupFileVersionCommand>(command => command.FileId == file.Id.Value
                    && command.StorageKey == (cancelled ? relocation.StorageKey.Value : sourceKey.Value)
                    && command.VersionId == (cancelled ? "destination" : "source")),
                TimeSpan.FromDays(7),
                ct);
        }
        else
        {
            files.DidNotReceiveWithAnyArgs().Update(default!);
            scheduler.ReceivedCalls().ShouldBeEmpty();
        }
        await storage.Received(kind is "latest" or "historical" ? 0 : 1).CopyAsync(
            sourceKey.Value,
            "source",
            relocation.StorageKey.Value,
            "text/plain",
            4,
            Arg.Is<IReadOnlyDictionary<string, string>>(values =>
                values[FileMetadataNames.RelocationAttemptId] == relocation.AttemptId.Value.ToString()),
            ct);
        await storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default!, ct);
    }

    [Fact(DisplayName = "Relocation should require persisted intent when file is ready")]
    public async Task RelocateAsync_Should_RequirePersistedIntent_When_FileIsReady()
    {
        var storage = Substitute.For<IFileStorage>();
        var file = File.Create(FileName.Create("a.txt").Value, FileType.Avatar, null).Value;
        var service = new FileRelocationService(
            Substitute.For<IFilesRepository>(),
            Substitute.For<IFoldersRepository>(),
            Substitute.For<IFolderHierarchyLock>(),
            storage,
            Substitute.For<IUnitOfWork>(),
            Substitute.For<IScheduler>());

        var result = await service.RelocateAsync(file, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        storage.ReceivedCalls().ShouldBeEmpty();
    }
}
