using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.ReconcileUploads;

public sealed class ReconcileFileUploadsHandlerTests
{
    private readonly IFileContentCleanupPolicy _contentCleanup = Substitute.For<IFileContentCleanupPolicy>();
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFileStorage _fileStorage = Substitute.For<IFileStorage>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public ReconcileFileUploadsHandlerTests()
    {
        _files.GetPendingUploadsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
        _files.GetExpiredUploadsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
        _files.GetFilesForDeletionAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<File>().ToAsyncEnumerable());
        _files.GetFilesWithUnusedContentsAsync(Arg.Any<CancellationToken>())
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
            .Returns(new FileMetadata("text/plain", 4));
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
            .Returns(contentType is null ? null : new FileMetadata(contentType, size));
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
                return new FileMetadata("text/plain", 4);
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
            .Returns(new FileMetadata("text/plain", 4));
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
            .Returns(new FileMetadata("text/plain", 4));
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
            .Returns(new FileMetadata("text/plain", 4));
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
            .Returns(new FileMetadata(contentType, size));

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
            .Returns(new FileMetadata("text/plain", 4));

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
            .Returns(contentExists ? new FileMetadata("text/plain", 4) : null);

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

    [Theory(DisplayName = "Reconciliation should delete only unused content when content is retired or abandoned")]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task HandleAsync_Should_DeleteOnlyUnusedContent_When_ContentIsRetiredOrAbandoned(
        bool retired,
        bool forget)
    {
        var file = CreateFile();
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var now = FileContentTimestamp.Create(DateTimeOffset.UtcNow);
        var upload = file.BeginContentUpload(now).Value;
        if (retired)
        {
            file.ReplaceContent(upload.Id, file.ContentType!, file.Size!, now);
        }
        var content = file.UnusedContents.Single();
        _files.GetFilesWithUnusedContentsAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());
        _contentCleanup.CanBeginDeletion(content).Returns(true);
        _contentCleanup.CanForget(content).Returns(forget);
        _fileStorage.GetMetadataAsync(content.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4));
        _fileStorage.DeleteAsync(content.StorageKey.Value, Arg.Any<CancellationToken>()).Returns(_ =>
        {
            content.DeletionStartedAt.ShouldNotBeNull();
            content.StorageKey.ShouldNotBe(file.StorageKey);
            if (!retired)
            {
                _unitOfWork.ReceivedCalls().Count().ShouldBe(1);
            }
            return Task.CompletedTask;
        });

        var result = await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.UnusedContents.Count.ShouldBe(forget ? 0 : 1);
        file.Status.ShouldBe(FileStatus.Ready);
        await _fileStorage.Received(1).DeleteAsync(content.StorageKey.Value, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Reconciliation should retain cleanup records when unused content storage fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RetainCleanupRecords_When_UnusedContentStorageFails(bool inspect)
    {
        var file = CreateFile();
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var now = FileContentTimestamp.Create(DateTimeOffset.UtcNow);
        var content = file.BeginContentUpload(now).Value;
        file.BeginContentDeletion(content.Id, now);
        _files.GetFilesWithUnusedContentsAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());
        _contentCleanup.CanBeginDeletion(content).Returns(true);
        _contentCleanup.CanForget(content).Returns(true);
        _fileStorage.GetMetadataAsync(content.StorageKey.Value, Arg.Any<CancellationToken>()).Returns(_ =>
            inspect ? throw new IOException("Inspection failed.") : new FileMetadata("text/plain", 4));
        _fileStorage.DeleteAsync(content.StorageKey.Value, Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Deletion failed."));

        var result = await CreateHandler().HandleAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.UnusedContents.ShouldHaveSingleItem().ShouldBeSameAs(content);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Reconciliation should preserve a fresh candidate when upload is still eligible")]
    public async Task HandleAsync_Should_PreserveFreshCandidate_When_UploadIsStillEligible()
    {
        var file = CreateFile();
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var upload = file.BeginContentUpload(FileContentTimestamp.Create(DateTimeOffset.UtcNow)).Value;
        _files.GetFilesWithUnusedContentsAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { file }.ToAsyncEnumerable());

        await CreateHandler().HandleAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);

        upload.DeletionStartedAt.ShouldBeNull();
        _fileStorage.ReceivedCalls().ShouldBeEmpty();
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
            logger: NullLogger<ReconcileFileUploadsHandler>.Instance,
            contentCleanupPolicy: _contentCleanup,
            timeProvider: TimeProvider.System);
    }
}
