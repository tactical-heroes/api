using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Rename;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Rename;

public sealed class RenameFileHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public RenameFileHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Rename file handler should preserve content metadata when rename is permitted")]
    [InlineData("Personal", "PendingUpload", "file-manager.personal-files.update.own", false)]
    [InlineData("Personal", "Ready", "file-manager.personal-files.update.all", true)]
    [InlineData("Avatar", "PendingUpload", "file-manager.avatars.update", false)]
    [InlineData("Avatar", "Ready", "file-manager.avatars.update", false)]
    public async Task HandleAsync_Should_PreserveContentMetadata_When_RenameIsPermitted(
        string type, string status, string permission, bool foreignOwner)
    {
        _user.HasPermission(permission).Returns(true);
        var file = CreateFile(type, foreignOwner ? Guid.NewGuid() : _userId);
        if (status == "Ready")
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        }
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        var key = file.StorageKey;
        var owner = file.UserId;
        var contentType = file.ContentType;
        var size = file.Size;

        var result = await CreateHandler().HandleAsync(
            new RenameFileCommand(file.Id.Value, " отчёт.txt "), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.Name.Value.ShouldBe("отчёт.txt");
        file.StorageKey.ShouldBeSameAs(key);
        file.UserId.ShouldBe(owner);
        file.Type.ShouldBe(FileType.FromName(type));
        file.Status.Name.ShouldBe(status);
        file.ContentType.ShouldBeSameAs(contentType);
        file.Size.ShouldBeSameAs(size);
        _files.Received(1).Update(file);
    }

    [Theory(DisplayName = "Rename file handler should conceal the file when update is denied")]
    [InlineData("Personal", false, "")]
    [InlineData("Personal", true, "file-manager.personal-files.update.own")]
    [InlineData("Personal", false, "file-manager.personal-files.read.all")]
    [InlineData("Avatar", false, "file-manager.avatars.read")]
    [InlineData("Avatar", false, "file-manager.personal-files.update.all")]
    public async Task HandleAsync_Should_ConcealFile_When_UpdateIsDenied(
        string type, bool foreignOwner, string permission)
    {
        _user.HasPermission(permission).Returns(true);
        var file = CreateFile(type, foreignOwner ? Guid.NewGuid() : _userId);
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var result = await CreateHandler().HandleAsync(
            new RenameFileCommand(file.Id.Value, "new.txt"), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        file.Name.Value.ShouldBe("file.txt");
        _files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Rename file handler should preserve the name when deletion has started")]
    [InlineData("Personal", "file-manager.personal-files.update.own")]
    [InlineData("Avatar", "file-manager.avatars.update")]
    public async Task HandleAsync_Should_PreserveName_When_DeletionHasStarted(string type, string permission)
    {
        _user.HasPermission(permission).Returns(true);
        var file = CreateFile(type, _userId);
        file.BeginDeletion();
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var result = await CreateHandler().HandleAsync(
            new RenameFileCommand(file.Id.Value, "new.txt"), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Conflict, "A file being deleted cannot be renamed.");
        file.Status.ShouldBe(FileStatus.Deleting);
        file.Name.Value.ShouldBe("file.txt");
        _files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Rename file handler should reject lookup when identifier is empty or missing")]
    [InlineData(false, ErrorType.NotFound)]
    [InlineData(true, ErrorType.Validation)]
    public async Task HandleAsync_Should_RejectLookup_When_IdentifierIsEmptyOrMissing(bool emptyId, ErrorType expected)
    {
        var command = new RenameFileCommand(emptyId ? Guid.Empty : Guid.NewGuid(), "new.txt");

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.Errors.ShouldContain(error => error.Type == expected);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
        if (emptyId)
        {
            _files.ReceivedCalls().ShouldBeEmpty();
        }
    }

    [Theory(DisplayName = "Rename file handler should preserve metadata when name is invalid")]
    [InlineData("")]
    [InlineData("../file.txt")]
    [InlineData("folder\\file.txt")]
    public async Task HandleAsync_Should_PreserveMetadata_When_NameIsInvalid(string name)
    {
        _user.HasPermission("file-manager.personal-files.update.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var result = await CreateHandler().HandleAsync(
            new RenameFileCommand(file.Id.Value, name), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        file.Name.Value.ShouldBe("file.txt");
        _files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    private static File CreateFile(string type, Guid ownerId)
    {
        var fileType = FileType.FromName(type);
        return File.Create(
            name: FileName.Create("file.txt").Value,
            type: fileType,
            userId: fileType == FileType.Personal ? UserId.Create(ownerId).Value : null).Value;
    }

    private RenameFileHandler CreateHandler()
    {
        return new RenameFileHandler(_files, new FileAccessPolicy(_user));
    }
}
