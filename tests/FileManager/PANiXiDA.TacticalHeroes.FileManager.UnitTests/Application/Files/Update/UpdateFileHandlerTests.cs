using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Messaging.Scheduling;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Update;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Update;

public sealed class UpdateFileHandlerTests
{
    [Fact(DisplayName = "Update file handler should protect destination lookup when hierarchy lock is pending")]
    public async Task HandleAsync_Should_ProtectDestinationLookup_When_HierarchyLockIsPending()
    {
        var actorId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.IsAuthenticated.Returns(true);
        user.UserId.Returns(actorId);
        user.HasPermission("file-manager.personal-files.update.own").Returns(true);
        var files = Substitute.For<IFilesRepository>();
        var folders = Substitute.For<IFoldersRepository>();
        var hierarchyLock = Substitute.For<IFolderHierarchyLock>();
        var file = File.Create(FileName.Create("before.txt").Value, FileType.Personal, UserId.Create(actorId).Value)
            .Value;
        var folder = Folder.Create(FolderName.Create("Target").Value, file.Type, file.UserId).Value;
        files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        var acquisition = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        hierarchyLock.AcquireAsync(Arg.Any<CancellationToken>()).Returns(acquisition.Task);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Func<CancellationToken, Task>>(0)(call.ArgAt<CancellationToken>(1)));
        var storage = Substitute.For<IFileStorage>();
        var policy = new FileAccessPolicy(user);
        var handler = new UpdateFileHandler(
            files,
            new FolderAccessService(folders, hierarchyLock, new FolderAccessPolicy(user, policy)),
            policy,
            unitOfWork,
            new FileRelocationService(
                files,
                storage,
                new FileStorageVersionService(storage),
                unitOfWork,
                Substitute.For<IScheduler>(),
                new FileRelocationOptions()));

        var pending = handler.HandleAsync(
            new UpdateFileCommand(file.Id.Value, "after.txt", "Personal", actorId, folder.Id.Value),
            TestContext.Current.CancellationToken);
        var folderWasRead = folders.ReceivedCalls().Any();
        var originalName = file.Name.Value;
        acquisition.SetResult();
        var result = await pending;

        folderWasRead.ShouldBeFalse();
        originalName.ShouldBe("before.txt");
        result.IsSuccess.ShouldBeTrue();
        file.Name.Value.ShouldBe("after.txt");
        file.FolderId.ShouldBe(folder.Id);
        files.Received(1).Update(file);
        folders.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Fact(DisplayName = "Update file handler should avoid persistence when command values are invalid")]
    public async Task HandleAsync_Should_AvoidPersistence_When_CommandValuesAreInvalid()
    {
        var files = Substitute.For<IFilesRepository>();
        var folders = Substitute.For<IFoldersRepository>();
        var hierarchyLock = Substitute.For<IFolderHierarchyLock>();
        var user = Substitute.For<ICurrentUser>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Func<CancellationToken, Task>>(0)(call.ArgAt<CancellationToken>(1)));
        var storage = Substitute.For<IFileStorage>();
        var policy = new FileAccessPolicy(user);
        var handler = new UpdateFileHandler(
            files,
            new FolderAccessService(folders, hierarchyLock, new FolderAccessPolicy(user, policy)),
            policy,
            unitOfWork,
            new FileRelocationService(
                files,
                storage,
                new FileStorageVersionService(storage),
                unitOfWork,
                Substitute.For<IScheduler>(),
                new FileRelocationOptions()));

        var result = await handler.HandleAsync(
            new UpdateFileCommand(Guid.Empty, "../file.txt", "unknown", Guid.Empty, Guid.Empty),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        files.ReceivedCalls().ShouldBeEmpty();
        folders.ReceivedCalls().ShouldBeEmpty();
        hierarchyLock.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "File update should reject oversized copies when file scope changes")]
    public async Task HandleAsync_Should_RejectOversizedCopiesBeforePersistence_When_FileScopeChanges()
    {
        var ct = TestContext.Current.CancellationToken;
        var user = Substitute.For<ICurrentUser>();
        user.IsAuthenticated.Returns(true);
        user.UserId.Returns(Guid.NewGuid());
        user.HasPermission(Arg.Any<string>()).Returns(true);
        var file = File.Create(FileName.Create("large.bin").Value, FileType.Avatar, null).Value;
        file.CompleteUpload(
            FileContentType.Create("application/octet-stream").Value,
            FileSize.Create(5L * 1024 * 1024 * 1024 + 1).Value,
            FileVersionId.Create("source").Value);
        var files = Substitute.For<IFilesRepository>();
        files.GetByIdAsync(file.Id, ct).Returns(file);
        var folders = Substitute.For<IFoldersRepository>();
        var hierarchyLock = Substitute.For<IFolderHierarchyLock>();
        var storage = Substitute.For<IFileStorage>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var policy = new FileAccessPolicy(user);
        var handler = new UpdateFileHandler(
            files,
            new FolderAccessService(folders, hierarchyLock, new FolderAccessPolicy(user, policy)),
            policy,
            unitOfWork,
            new FileRelocationService(
                files,
                storage,
                new FileStorageVersionService(storage),
                unitOfWork,
                Substitute.For<IScheduler>(),
                new FileRelocationOptions()));

        var result = await handler.HandleAsync(
            new UpdateFileCommand(file.Id.Value, "after.bin", "Personal", Guid.NewGuid()),
            ct);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        file.OperationAttemptId.ShouldBeNull();
        file.Type.ShouldBe(FileType.Avatar);
        unitOfWork.ReceivedCalls().ShouldBeEmpty();
        storage.ReceivedCalls().ShouldBeEmpty();
        files.DidNotReceiveWithAnyArgs().Update(default!);
    }
}
