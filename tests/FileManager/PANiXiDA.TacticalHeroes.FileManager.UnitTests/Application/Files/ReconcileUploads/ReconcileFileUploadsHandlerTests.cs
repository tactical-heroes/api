using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.ReconcileUploads;

public sealed class ReconcileFileUploadsHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFileStorage _fileStorage = Substitute.For<IFileStorage>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public ReconcileFileUploadsHandlerTests()
    {
        _fileStorage.GetVersionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<FileVersion>().ToAsyncEnumerable());
        _files.GetPendingUploadsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
        _unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Func<CancellationToken, Task>>(0)(call.ArgAt<CancellationToken>(1)));
        _files.GetExpiredContentUpdatesAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
        _files.GetUpdatingFilesAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
        _files.GetExpiredUploadsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
        _files.GetFilesForDeletionAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
        _files.GetDeletedFilesAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
    }

    [Fact(DisplayName = "Handler should finalize actual metadata when stored content exists")]
    public async Task HandleAsync_Should_FinalizeActualMetadata_When_StoredContentExists()
    {
        var file = CreateFile();
        SetFiles(file);
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4, ReadOnlyDictionary<string, string>.Empty, "initial-version"));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType!.Value.ShouldBe("text/plain");
        file.Size!.Value.ShouldBe(4);
        _files.Received(1).Update(file);
        await _fileStorage.Received(1).GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
        await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Handler should keep pending state when metadata is missing or invalid")]
    [InlineData(null, 0)]
    [InlineData("text/plain", 0)]
    [InlineData("text/*", 4)]
    public async Task HandleAsync_Should_KeepPendingState_When_MetadataIsMissingOrInvalid(
        string? contentType,
        long size)
    {
        var file = CreateFile();
        SetFiles(file);
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(contentType is null ? null : new FileMetadata(
                contentType, size, ReadOnlyDictionary<string, string>.Empty, "initial-version"));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.PendingUpload);
        _files.DidNotReceive().Update(Arg.Any<File>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should preserve deletion state when deletion begins during the metadata request")]
    public async Task HandleAsync_Should_PreserveDeletionState_When_DeletionBeginsDuringTheMetadataRequest()
    {
        var file = CreateFile();
        SetFiles(file);
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                file.BeginDeletion();
                return new FileMetadata("text/plain", 4, ReadOnlyDictionary<string, string>.Empty, "initial-version");
            });
        var handler = CreateHandler();

        var result = await handler.HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Deleting);
        _files.DidNotReceive().Update(Arg.Any<File>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should continue processing when an earlier object is missing")]
    public async Task HandleAsync_Should_ContinueProcessing_When_AnEarlierObjectIsMissing()
    {
        var missing = CreateFile();
        var uploaded = CreateFile();
        SetFiles(missing, uploaded);
        _fileStorage.GetMetadataAsync(uploaded.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4, ReadOnlyDictionary<string, string>.Empty, "initial-version"));
        var handler = CreateHandler();

        await handler.HandleAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        missing.Status.ShouldBe(FileStatus.PendingUpload);
        uploaded.Status.ShouldBe(FileStatus.Ready);
        _files.Received(1).GetPendingUploadsAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Handler should continue processing when storage fails for one file")]
    public async Task HandleAsync_Should_ContinueProcessing_When_StorageFailsForOneFile()
    {
        var failed = CreateFile();
        var uploaded = CreateFile();
        SetFiles(failed, uploaded);
        _fileStorage.GetMetadataAsync(failed.StorageKey.Value, Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Storage unavailable."));
        _fileStorage.GetMetadataAsync(uploaded.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4, ReadOnlyDictionary<string, string>.Empty, "initial-version"));
        var handler = CreateHandler();

        await handler.HandleAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        failed.Status.ShouldBe(FileStatus.PendingUpload);
        uploaded.Status.ShouldBe(FileStatus.Ready);
        await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Handler should propagate cancellation when cancellation is requested")]
    public async Task HandleAsync_Should_PropagateCancellation_When_CancellationIsRequested()
    {
        SetFiles(CreateFile());
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        var handler = CreateHandler();

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new ReconcileFileUploadsCommand(), cancellation.Token));

        exception.ShouldBeAssignableTo<OperationCanceledException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should stop the pass when saving encounters a version conflict")]
    public async Task HandleAsync_Should_StopThePass_When_SavingEncountersAVersionConflict()
    {
        var conflicted = CreateFile();
        var next = CreateFile();
        SetFiles(conflicted, next);
        _fileStorage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4, ReadOnlyDictionary<string, string>.Empty, "initial-version"));
        var failure = new ConcurrencyException();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(failure);
        var handler = CreateHandler();

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken));

        exception.ShouldBeSameAs(failure);
        next.Status.ShouldBe(FileStatus.PendingUpload);
        await _fileStorage.DidNotReceive().GetMetadataAsync(next.StorageKey.Value, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should delete an expired upload when stored content is missing")]
    public async Task HandleAsync_Should_DeleteExpiredUpload_When_StoredContentIsMissing()
    {
        var file = CreateFile();
        _files.GetExpiredUploadsAsync(Arg.Any<CancellationToken>()).Returns(new[] { file }.ToAsyncEnumerable());
        _files.GetFilesForDeletionAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());
        var savedStates = new List<FileStatus>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            savedStates.Add(file.Status);
            return Task.CompletedTask;
        });

        var result = await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        savedStates.ShouldBe(new[] { FileStatus.Deleting, FileStatus.Deleting });
        _files.Received(1).Delete(file);
        _files.Received(1).Update(file);
        await _fileStorage.Received(1).DeleteAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Handler should delete an expired upload when stored metadata is invalid")]
    [InlineData(0, "text/plain")]
    [InlineData(4, "text/*")]
    public async Task HandleAsync_Should_DeleteExpiredUpload_When_StoredMetadataIsInvalid(long size, string contentType)
    {
        var file = CreateFile();
        _files.GetExpiredUploadsAsync(Arg.Any<CancellationToken>()).Returns(new[] { file }.ToAsyncEnumerable());
        _files.GetFilesForDeletionAsync(Arg.Any<CancellationToken>()).Returns(new[] { file }.ToAsyncEnumerable());
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata(contentType, size, ReadOnlyDictionary<string, string>.Empty, "initial-version"));

        await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        file.Status.ShouldBe(FileStatus.Deleting);
        _files.Received(1).Delete(file);
        await _fileStorage.Received(1).DeleteAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
        await _unitOfWork.Received(2).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Handler should recover an expired upload when valid stored content exists")]
    public async Task HandleAsync_Should_RecoverExpiredUpload_When_ValidStoredContentExists()
    {
        var file = CreateFile();
        _files.GetExpiredUploadsAsync(Arg.Any<CancellationToken>()).Returns(new[] { file }.ToAsyncEnumerable());
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4, ReadOnlyDictionary<string, string>.Empty, "initial-version"));

        await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        file.Status.ShouldBe(FileStatus.Ready);
        await _fileStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Handler should preserve an expired upload when metadata inspection fails")]
    public async Task HandleAsync_Should_PreserveExpiredUpload_When_MetadataInspectionFails()
    {
        var file = CreateFile();
        _files.GetExpiredUploadsAsync(Arg.Any<CancellationToken>()).Returns(new[] { file }.ToAsyncEnumerable());
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Storage unavailable."));

        await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        file.Status.ShouldBe(FileStatus.PendingUpload);
        await _fileStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should stop before storage deletion when persisting deletion intent conflicts")]
    public async Task HandleAsync_Should_StopBeforeStorageDeletion_When_PersistingDeletionIntentConflicts()
    {
        var file = CreateFile();
        _files.GetExpiredUploadsAsync(Arg.Any<CancellationToken>()).Returns(new[] { file }.ToAsyncEnumerable());
        _files.GetFilesForDeletionAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new ConcurrencyException());

        var exception = await Record.ExceptionAsync(() => CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken));

        exception.ShouldBeOfType<ConcurrencyException>();
        await _fileStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should continue deletion processing when an earlier storage deletion fails")]
    public async Task HandleAsync_Should_ContinueDeletionProcessing_When_AnEarlierStorageDeletionFails()
    {
        var failed = CreateFile();
        var next = CreateFile();
        failed.BeginDeletion();
        next.BeginDeletion();
        _files.GetFilesForDeletionAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { failed, next }.ToAsyncEnumerable());
        _fileStorage.DeleteAsync(failed.StorageKey.Value, Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Storage unavailable."));

        await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        failed.Status.ShouldBe(FileStatus.Deleting);
        next.Status.ShouldBe(FileStatus.Deleting);
        _files.DidNotReceive().Delete(failed);
        _files.Received(1).Delete(next);
        await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Handler should audit a deleted file when late stored content may exist")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_AuditDeletedFile_When_LateStoredContentMayExist(bool contentExists)
    {
        var file = CreateFile();
        file.BeginDeletion();
        _files.GetDeletedFilesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(contentExists ? new FileMetadata(
                "text/plain", 4, ReadOnlyDictionary<string, string>.Empty, "initial-version") : null);

        await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        file.Status.ShouldBe(FileStatus.Deleting);
        _files.DidNotReceive().Delete(Arg.Any<File>());
        await _fileStorage.Received(contentExists ? 1 : 0)
            .DeleteAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should propagate cancellation when deleting stored content is canceled")]
    public async Task HandleAsync_Should_PropagateCancellation_When_DeletingStoredContentIsCanceled()
    {
        var file = CreateFile();
        file.BeginDeletion();
        _files.GetFilesForDeletionAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        _fileStorage.DeleteAsync(file.StorageKey.Value, cancellation.Token).Returns(async _ =>
        {
            await cancellation.CancelAsync();
            cancellation.Token.ThrowIfCancellationRequested();
        });

        var exception = await Record.ExceptionAsync(() => CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), cancellation.Token));

        exception.ShouldBeAssignableTo<OperationCanceledException>();
        file.Status.ShouldBe(FileStatus.Deleting);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory(DisplayName = "Reconciliation should activate only matching valid content when an update exists")]
    [InlineData("text/plain", 4, "matching", true)]
    [InlineData("image/png", 9, "matching", true)]
    [InlineData("invalid", 4, "matching", false)]
    [InlineData("text/plain", 0, "matching", false)]
    [InlineData("text/plain", 4, "other", false)]
    [InlineData("text/plain", 4, "missing", false)]
    [InlineData("text/plain", 4, "versionless", false)]
    public async Task HandleAsync_Should_ActivateMatchingContent_When_AnUpdateExists(
        string contentType, long size, string condition, bool completed)
    {
        var file = CreateFile();
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var key = file.StorageKey;
        var operation = file.BeginContentUpdate(FileVersionId.Create("initial").Value).Value;
        _files.GetUpdatingFilesAsync(Arg.Any<CancellationToken>()).Returns(new[] { file }.ToAsyncEnumerable());
        _fileStorage.GetMetadataAsync(key.Value, Arg.Any<CancellationToken>())
            .Returns(condition == "missing" ? null : new FileMetadata(
                contentType, size,
                new Dictionary<string, string>
                {
                    [FileMetadataNames.ContentUpdateId] =
                        (condition == "other" ? Guid.NewGuid() : operation).ToString("D")
                },
                condition == "versionless" ? null : "replacement"));

        var result = await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(completed ? FileStatus.Ready : FileStatus.Updating);
        file.StorageKey.ShouldBe(key);
        file.VersionId!.Value.ShouldBe(completed ? "replacement" : "initial");
        file.Size!.Value.ShouldBe(completed ? size : 4);
        await _unitOfWork.Received(completed ? 1 : 0).SaveChangesAsync(TestContext.Current.CancellationToken);
        await _fileStorage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Reconciliation should find the matching version when a late upload becomes current")]
    public async Task HandleAsync_Should_FindMatchingVersion_When_ALateUploadBecomesCurrent()
    {
        var file = CreateFile();
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var operation = file.BeginContentUpdate(FileVersionId.Create("initial").Value).Value;
        _files.GetUpdatingFilesAsync(Arg.Any<CancellationToken>()).Returns(new[] { file }.ToAsyncEnumerable());
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 1, ReadOnlyDictionary<string, string>.Empty, "late"));
        _fileStorage.GetVersionsAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new FileVersion("late", DateTimeOffset.UtcNow),
                new FileVersion("initial", DateTimeOffset.UtcNow.AddMinutes(-2)),
                new FileVersion("replacement", DateTimeOffset.UtcNow.AddMinutes(-1))
            }.ToAsyncEnumerable());
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, "replacement", Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("image/png", 9,
                new Dictionary<string, string> { [FileMetadataNames.ContentUpdateId] = operation.ToString("D") },
                "replacement"));

        await CreateHandler().HandleAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        file.Status.ShouldBe(FileStatus.Ready);
        file.VersionId!.Value.ShouldBe("replacement");
        file.Size!.Value.ShouldBe(9);
        await _fileStorage.DidNotReceive().GetMetadataAsync(
            file.StorageKey.Value, "initial", TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Reconciliation should restore old content when an unmatched attempt expires")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RestoreOldContent_When_AnUnmatchedAttemptExpires(bool invalid)
    {
        var file = CreateFile();
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var key = file.StorageKey;
        file.BeginContentUpdate(FileVersionId.Create("initial").Value);
        _files.GetExpiredContentUpdatesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());
        _fileStorage.GetMetadataAsync(key.Value, Arg.Any<CancellationToken>())
            .Returns(invalid ? new FileMetadata("invalid", 0,
                ReadOnlyDictionary<string, string>.Empty, "bad") : null);

        await CreateHandler().HandleAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        file.Status.ShouldBe(FileStatus.Ready);
        file.StorageKey.ShouldBe(key);
        file.ContentUpdateId.ShouldBeNull();
        file.VersionId!.Value.ShouldBe("initial");
        file.Size!.Value.ShouldBe(4);
        _files.DidNotReceive().Delete(file);
    }

    [Theory(DisplayName = "Reconciliation should retain updating when storage inspection fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RetainUpdating_When_StorageInspectionFails(bool listing)
    {
        var file = CreateFile();
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        file.BeginContentUpdate(FileVersionId.Create("initial").Value);
        _files.GetExpiredContentUpdatesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());
        if (listing)
        {
            _fileStorage.GetVersionsAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
                .Returns(_ => throw new IOException("Listing failed."));
        }
        else
        {
            _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
                .ThrowsAsync(new IOException("Inspection failed."));
        }

        await CreateHandler().HandleAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        file.Status.ShouldBe(FileStatus.Updating);
        _unitOfWork.ReceivedCalls().ShouldBeEmpty();
    }

    private static File CreateFile()
    {
        return File.Create(
            name: FileName.Create("file.txt").Value,
            type: FileType.Personal,
            userId: UserId.Create(Guid.NewGuid()).Value).Value;
    }

    private void SetFiles(params File[] files)
    {
        _files.GetPendingUploadsAsync(Arg.Any<CancellationToken>()).Returns(files.ToAsyncEnumerable());
    }

    private ReconcileFileUploadsHandler CreateHandler()
    {
        return new ReconcileFileUploadsHandler(
            filesRepository: _files,
            fileStorage: _fileStorage,
            unitOfWork: _unitOfWork,
            logger: NullLogger<ReconcileFileUploadsHandler>.Instance);
    }
}
