using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CreatePersonal;

public sealed class CreatePersonalFileHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public CreatePersonalFileHandlerTests()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Personal file upload should commit pending metadata before uploading content")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_CommitPendingThenUploadThenSaveReady_When_CommandIsValid(bool hasFolder)
    {
        await using var content = new MemoryStream([1, 2, 3]);
        var command = CreateCommand(content);
        var folder = Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.Personal,
            UserId.Create(_userId).Value).Value;
        if (hasFolder)
        {
            command = command with { FolderId = folder.Id.Value };
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        }

        var steps = new List<string>();
        _files.AddAsync(Arg.Any<File>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<File>().Status.ShouldBe(FileStatus.PendingUpload);
            steps.Add("pending");
            return Task.CompletedTask;
        });
        _unitOfWork.CommitTransactionAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            steps.Add("commit");
            return Task.CompletedTask;
        });
        _storage.UploadAsync(Arg.Any<string>(), content, "text/plain", Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                steps.Add("upload");
                return Task.CompletedTask;
            });
        _files.UpdateAsync(Arg.Any<File>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var file = call.Arg<File>();
            file.Status.ShouldBe(FileStatus.Ready);
            file.UserId.ShouldBe(UserId.Create(_userId).Value);
            file.FolderId.ShouldBe(hasFolder ? folder.Id : null);
            file.StorageKey.Value.ShouldBe($"personal/{_userId:D}/{file.Id.Value:D}");
            file.Size!.Value.ShouldBe(content.Length);
            steps.Add("ready");
            return Task.CompletedTask;
        });

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        steps.ShouldBe(["pending", "commit", "upload", "ready"]);
        content.CanRead.ShouldBeTrue();
    }

    [Theory(DisplayName = "Personal file upload should reject an unavailable folder without uploading")]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("avatar")]
    public async Task HandleAsync_Should_ReturnNotFoundWithoutUploading_When_FolderIsUnavailable(string folderKind)
    {
        await using var content = new MemoryStream([1]);
        var folder = Folder.Create(
            FolderName.Create("Documents").Value,
            folderKind == "avatar" ? FileType.Avatar : FileType.Personal,
            folderKind == "avatar" ? null : UserId.Create(Guid.NewGuid()).Value).Value;
        if (folderKind != "missing")
        {
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        }

        var command = CreateCommand(content) with { FolderId = folder.Id.Value };

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Single().Message.ShouldBe("Folder was not found.");
        await _files.DidNotReceiveWithAnyArgs().AddAsync(null!, TestContext.Current.CancellationToken);
        await _storage.DidNotReceiveWithAnyArgs()
            .UploadAsync(null!, null!, null!, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Create personal file handler should reject input before saving when command is invalid")]
    [InlineData("metadata")]
    [InlineData("folder")]
    [InlineData("stream")]
    public async Task HandleAsync_Should_RejectInvalidInput_When_CommandIsInvalid(string invalidPart)
    {
        await using var content = new MemoryStream([1]);
        var command = CreateCommand(content);
        if (invalidPart == "metadata")
        {
            command = command with { Name = "../file", ContentType = "text/*", Size = 0 };
        }
        else if (invalidPart == "folder")
        {
            command = command with { FolderId = Guid.Empty };
        }
        else
        {
            content.Close();
        }

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        await _files.DidNotReceiveWithAnyArgs().AddAsync(null!, TestContext.Current.CancellationToken);
        await _storage.DidNotReceiveWithAnyArgs()
            .UploadAsync(null!, null!, null!, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Create personal file handler should reject the caller when user id is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnUnauthorizedWithoutSaving_When_UserIdIsUnavailable(bool emptyUserId)
    {
        _currentUser.UserId.Returns(emptyUserId ? Guid.Empty : null);
        await using var content = new MemoryStream([1]);

        var result = await CreateHandler().HandleAsync(CreateCommand(content), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        _files.ReceivedCalls().ShouldBeEmpty();
        _folders.ReceivedCalls().ShouldBeEmpty();
        _storage.ReceivedCalls().ShouldBeEmpty();
        _unitOfWork.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Create personal file handler should not upload when commit fails")]
    public async Task HandleAsync_Should_PropagateFailureWithoutUploading_When_CommitFails()
    {
        await using var content = new MemoryStream([1]);
        _unitOfWork.CommitTransactionAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new IOException("Commit failed."));

        var exception = await Record.ExceptionAsync(() =>
            CreateHandler().HandleAsync(CreateCommand(content), TestContext.Current.CancellationToken));

        exception.ShouldBeOfType<IOException>();
        await _storage.DidNotReceiveWithAnyArgs()
            .UploadAsync(null!, null!, null!, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Create personal file handler should not mark the file ready when upload fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PropagateFailureWithoutCompleting_When_UploadFails(bool cancelled)
    {
        await using var content = new MemoryStream([1]);
        var cancellationToken = TestContext.Current.CancellationToken;
        Exception failure = cancelled
            ? new OperationCanceledException(cancellationToken)
            : new IOException("Upload failed.");
        _storage.UploadAsync(Arg.Any<string>(), content, Arg.Any<string>(), cancellationToken).ThrowsAsync(failure);

        var thrown = await Record.ExceptionAsync(
            () => CreateHandler().HandleAsync(CreateCommand(content), cancellationToken));

        thrown.ShouldBeSameAs(failure);
        await _unitOfWork.Received(1).CommitTransactionAsync(cancellationToken);
        await _files.DidNotReceiveWithAnyArgs().UpdateAsync(null!, cancellationToken);
        content.CanRead.ShouldBeTrue();
    }

    private static CreatePersonalFileCommand CreateCommand(Stream content)
    {
        return new CreatePersonalFileCommand(
            "file.txt", "text/plain", content.Length, content, FolderId: null);
    }

    private CreatePersonalFileHandler CreateHandler()
    {
        return new CreatePersonalFileHandler(_files, _folders, _storage, _unitOfWork, _currentUser);
    }
}
