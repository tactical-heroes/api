using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.ReconcilePersonalUploads;

public sealed class ReconcilePersonalFileUploadsHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFilesReadRepository _readFiles = Substitute.For<IFilesReadRepository>();
    private readonly IFileStorage _fileStorage = Substitute.For<IFileStorage>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    public ReconcilePersonalFileUploadsHandlerTests()
    {
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "Handler should finalize actual metadata when stored content exists")]
    public async Task HandleAsync_Should_FinalizeActualMetadata_When_StoredContentExists()
    {
        var file = CreateFile();
        SetPage(file);
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType!.Value.ShouldBe("text/plain");
        file.Size!.Value.ShouldBe(4);
        _files.Received(1).Update(file);
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
        SetPage(file);
        _fileStorage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(contentType is null ? null : new FileMetadata(contentType, size));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.PendingUpload);
        _files.DidNotReceive().Update(Arg.Any<File>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory(DisplayName = "Handler should skip completion when the aggregate no longer matches the candidate")]
    [InlineData("missing")]
    [InlineData("ready")]
    [InlineData("deleting")]
    [InlineData("key")]
    public async Task HandleAsync_Should_SkipCompletion_When_TheAggregateNoLongerMatchesTheCandidate(string condition)
    {
        var file = CreateFile();
        SetPage(file);
        if (condition is "ready" or "deleting")
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
            if (condition == "deleting")
            {
                file.BeginDeletion();
            }
        }

        if (condition == "missing")
        {
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns((File?)null);
        }
        else if (condition == "key")
        {
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(CreateFile());
        }

        _fileStorage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _files.DidNotReceive().Update(Arg.Any<File>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should continue paging when an earlier object is missing")]
    public async Task HandleAsync_Should_ContinuePaging_When_AnEarlierObjectIsMissing()
    {
        var missing = CreateFile();
        var uploaded = CreateFile();
        _readFiles.GetPendingPersonalUploadsAsync(
            createdBefore: Arg.Any<DateTime>(),
            afterId: Arg.Any<Guid>(),
            batchSize: Arg.Any<int>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Guid>(1) == Guid.Empty
                ? [new PendingPersonalFileUploadReadModel(missing.Id.Value, missing.StorageKey.Value)]
                : call.ArgAt<Guid>(1) == missing.Id.Value
                    ? [new PendingPersonalFileUploadReadModel(uploaded.Id.Value, uploaded.StorageKey.Value)] : []);
        _files.GetByIdAsync(uploaded.Id, Arg.Any<CancellationToken>()).Returns(uploaded);
        _fileStorage.GetMetadataAsync(uploaded.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4));
        var handler = CreateHandler();

        await handler.HandleAsync(new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        missing.Status.ShouldBe(FileStatus.PendingUpload);
        uploaded.Status.ShouldBe(FileStatus.Ready);
        await _readFiles.Received(3).GetPendingPersonalUploadsAsync(
            createdBefore: Arg.Any<DateTime>(),
            afterId: Arg.Any<Guid>(),
            batchSize: 100,
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Handler should continue processing when storage fails for one file")]
    public async Task HandleAsync_Should_ContinueProcessing_When_StorageFailsForOneFile()
    {
        var failed = CreateFile();
        var uploaded = CreateFile();
        SetPage(failed, uploaded);
        _fileStorage.GetMetadataAsync(failed.StorageKey.Value, Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Storage unavailable."));
        _fileStorage.GetMetadataAsync(uploaded.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4));
        var handler = CreateHandler();

        await handler.HandleAsync(new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        failed.Status.ShouldBe(FileStatus.PendingUpload);
        uploaded.Status.ShouldBe(FileStatus.Ready);
        await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Handler should use a fixed cutoff when loading old pending uploads")]
    public async Task HandleAsync_Should_UseFixedCutoff_When_LoadingOldPendingUploads()
    {
        var now = new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
        _timeProvider.GetUtcNow().Returns(now);
        SetPage(CreateFile());
        var handler = CreateHandler();

        await handler.HandleAsync(new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        await _readFiles.Received(2).GetPendingPersonalUploadsAsync(
            createdBefore: now.AddMinutes(-15).UtcDateTime,
            afterId: Arg.Any<Guid>(),
            batchSize: 100,
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Handler should propagate cancellation when cancellation is requested")]
    public async Task HandleAsync_Should_PropagateCancellation_When_CancellationIsRequested()
    {
        SetPage(CreateFile());
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        var handler = CreateHandler();

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), cancellation.Token));

        exception.ShouldBeAssignableTo<OperationCanceledException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handler should stop the pass when saving encounters a version conflict")]
    public async Task HandleAsync_Should_StopThePass_When_SavingEncountersAVersionConflict()
    {
        var conflicted = CreateFile();
        var next = CreateFile();
        SetPage(conflicted, next);
        _fileStorage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4));
        var failure = new ConcurrencyException();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(failure);
        var handler = CreateHandler();

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken));

        exception.ShouldBeSameAs(failure);
        next.Status.ShouldBe(FileStatus.PendingUpload);
        await _fileStorage.DidNotReceive().GetMetadataAsync(next.StorageKey.Value, Arg.Any<CancellationToken>());
    }

    private static File CreateFile()
    {
        return File.Create(
            name: FileName.Create("file.txt").Value,
            type: FileType.Personal,
            userId: UserId.Create(Guid.NewGuid()).Value).Value;
    }

    private void SetPage(params File[] files)
    {
        foreach (var file in files)
        {
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        }

        var page = files.Select(file => new PendingPersonalFileUploadReadModel(file.Id.Value, file.StorageKey.Value))
            .ToArray();
        _readFiles.GetPendingPersonalUploadsAsync(
            createdBefore: Arg.Any<DateTime>(),
            afterId: Arg.Any<Guid>(),
            batchSize: Arg.Any<int>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Guid>(1) == Guid.Empty ? page : []);
    }

    private ReconcilePersonalFileUploadsHandler CreateHandler()
    {
        return new ReconcilePersonalFileUploadsHandler(
            filesRepository: _files,
            filesReadRepository: _readFiles,
            fileStorage: _fileStorage,
            unitOfWork: _unitOfWork,
            timeProvider: _timeProvider,
            options: Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions()),
            logger: NullLogger<ReconcilePersonalFileUploadsHandler>.Instance);
    }
}
