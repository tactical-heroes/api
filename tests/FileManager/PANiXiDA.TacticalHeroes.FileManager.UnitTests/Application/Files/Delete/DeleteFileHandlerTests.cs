using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Delete;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Delete;

public sealed class DeleteFileHandlerTests
{
    [Theory(DisplayName = "Delete file handler should persist deletion intent when the matching permission is granted")]
    [InlineData("Personal", "PendingUpload")]
    [InlineData("Personal", "Ready")]
    [InlineData("Personal", "Deleting")]
    [InlineData("Avatar", "PendingUpload")]
    [InlineData("Avatar", "Ready")]
    [InlineData("Avatar", "Deleting")]
    public async Task HandleAsync_Should_PersistDeletionIntent_When_TheMatchingPermissionIsGranted(
        string type, string status)
    {
        var currentUser = CreateCurrentUser();
        currentUser.HasPermission(type == "Personal"
            ? "file-manager.personal-files.delete.own"
            : "file-manager.avatars.delete")
            .Returns(true);
        var file = CreateFile(type, currentUser.UserId!.Value);
        if (status == "Ready")
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        }
        else if (status == "Deleting")
        {
            file.BeginDeletion();
        }
        var repository = Substitute.For<IFilesRepository>();
        var ct = TestContext.Current.CancellationToken;
        repository.GetByIdAsync(file.Id, ct).Returns(file);
        var handler = new DeleteFileHandler(repository, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(new DeleteFileCommand(file.Id.Value), ct);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Deleting);
        await repository.Received(1).GetByIdAsync(file.Id, ct);
        repository.Received(1).Update(file);
        repository.DidNotReceive().Delete(Arg.Any<File>());
    }

    [Theory(DisplayName = "Delete file handler should hide file and preserve state when access is denied")]
    [InlineData("Personal", false, "file-manager.personal-files.delete.own")]
    [InlineData("Personal", true, "file-manager.personal-files.read.all")]
    [InlineData("Personal", true, "file-manager.avatars.delete")]
    [InlineData("Avatar", false, "file-manager.personal-files.delete.all")]
    [InlineData("Avatar", false, "file-manager.avatars.read")]
    public async Task HandleAsync_Should_HideFileAndPreserveState_When_AccessIsDenied(
        string type, bool ownsFile, string permission)
    {
        var currentUser = CreateCurrentUser();
        currentUser.HasPermission(permission).Returns(true);
        var file = CreateFile(type, ownsFile ? currentUser.UserId!.Value : Guid.NewGuid());
        var repository = Substitute.For<IFilesRepository>();
        var ct = TestContext.Current.CancellationToken;
        repository.GetByIdAsync(file.Id, ct).Returns(file);
        var handler = new DeleteFileHandler(repository, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(new DeleteFileCommand(file.Id.Value), ct);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        file.Status.ShouldBe(FileStatus.PendingUpload);
        repository.DidNotReceive().Update(Arg.Any<File>());
        repository.DidNotReceive().Delete(Arg.Any<File>());
    }

    [Fact(DisplayName = "Delete file handler should return not found when file is absent")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FileIsAbsent()
    {
        var repository = Substitute.For<IFilesRepository>();
        var handler = new DeleteFileHandler(repository, new FileAccessPolicy(CreateCurrentUser()));

        var result = await handler.HandleAsync(
            new DeleteFileCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        repository.DidNotReceive().Update(Arg.Any<File>());
    }

    [Fact(DisplayName = "Delete file handler should reject the identifier when id is empty")]
    public async Task HandleAsync_Should_RejectIdentifier_When_IdIsEmpty()
    {
        var repository = Substitute.For<IFilesRepository>();
        var handler = new DeleteFileHandler(repository, new FileAccessPolicy(CreateCurrentUser()));

        var result = await handler.HandleAsync(
            new DeleteFileCommand(Guid.Empty),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    private static ICurrentUser CreateCurrentUser()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        currentUser.IsAuthenticated.Returns(true);
        return currentUser;
    }

    private static File CreateFile(string type, Guid userId)
    {
        return File.Create(
            FileName.Create("file.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(userId).Value : null).Value;
    }
}
