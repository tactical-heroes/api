using PANiXiDA.Core.Application.Messaging.Scheduling;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Constants;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options.FileRelocation;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
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
    public async Task RelocateAsync_Should_RetainActiveContentUntilValidatedCopy_When_RecoveryPathsVary(string kind)
    {
        var ct = TestContext.Current.CancellationToken;
        var files = Substitute.For<IFilesRepository>();
        var storage = Substitute.For<IFileStorage>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var scheduler = Substitute.For<IScheduler>();
        var options = new FileRelocationOptions { CleanupDelay = TimeSpan.FromDays(2) };
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
        var attemptId = file.OperationAttemptId!;
        var destinationKey = FileStorageKey.Create(id: file.Id, type: file.Type, userId: file.UserId).Value;
        var sourceKey = file.StorageKey;
        var metadata = new FileMetadata(
            "text/plain",
            4,
            new Dictionary<string, string>
            {
                [FileMetadataNames.OperationAttemptId] = attemptId.Value.ToString()
            },
            "destination");
        var current = new FileMetadata("text/plain", 4, new Dictionary<string, string>(), "unrelated");
        storage.GetMetadataAsync(destinationKey.Value, ct).Returns(kind == "latest" ? metadata : current);
        FileVersion[] versions = kind == "historical"
            ? [
                new FileVersion("unrelated", DateTimeOffset.UnixEpoch),
                new FileVersion("destination", DateTimeOffset.UnixEpoch)
            ]
            : [];
        storage.GetVersionsAsync(destinationKey.Value, ct).Returns(versions.ToAsyncEnumerable());
        storage.GetMetadataAsync(destinationKey.Value, "destination", ct)
            .Returns(kind == "wrong-metadata" ? metadata with { Size = 9 }
                : kind == "wrong-version" ? metadata with { VersionId = "other" } : metadata);
        storage.CopyAsync(
            sourceKey.Value,
            "source",
            destinationKey.Value,
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
        var service = new FileRelocationService(
            files,
            storage,
            new FileStorageVersionService(storage),
            unitOfWork,
            scheduler,
            options);

        var result = await service.RelocateAsync(file, ct);

        var unusable = kind is "missing-version" or "wrong-version" or "wrong-metadata";
        result.IsSuccess.ShouldBe(!unusable);
        file.Status.ShouldBe(unusable ? FileStatus.Relocating : FileStatus.Ready);
        file.StorageKey.ShouldBe(unusable ? sourceKey : destinationKey);
        file.VersionId!.Value.ShouldBe(unusable ? "source" : "destination");
        file.Name.Value.ShouldBe("new.txt");
        file.Type.ShouldBe(FileType.Personal);
        file.UserId.ShouldBe(owner);
        if (!unusable)
        {
            file.OperationAttemptId.ShouldBeNull();
            files.Received(1).Update(file);
            await scheduler.Received(1).ScheduleAsync(
                Arg.Is<CleanupFileVersionCommand>(command => command.FileId == file.Id.Value
                    && command.StorageKey == sourceKey.Value
                    && command.VersionId == "source"),
                options.CleanupDelay,
                ct);
        }
        else
        {
            file.OperationAttemptId.ShouldBe(attemptId);
            files.DidNotReceiveWithAnyArgs().Update(default!);
            scheduler.ReceivedCalls().ShouldBeEmpty();
        }
        await storage.Received(kind is "latest" or "historical" ? 0 : 1).CopyAsync(
            sourceKey.Value,
            "source",
            destinationKey.Value,
            "text/plain",
            4,
            Arg.Is<IReadOnlyDictionary<string, string>>(values =>
                values[FileMetadataNames.OperationAttemptId] == attemptId.Value.ToString()),
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
            storage,
            new FileStorageVersionService(storage),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<IScheduler>(),
            new FileRelocationOptions());

        var result = await service.RelocateAsync(file, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        storage.ReceivedCalls().ShouldBeEmpty();
    }
}
