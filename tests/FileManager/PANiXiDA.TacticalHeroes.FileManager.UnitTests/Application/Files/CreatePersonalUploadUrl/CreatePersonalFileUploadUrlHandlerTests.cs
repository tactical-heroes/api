using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonalUploadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CreatePersonalUploadUrl;

public sealed class CreatePersonalFileUploadUrlHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IFolderHierarchyLock _hierarchyLock = Substitute.For<IFolderHierarchyLock>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public CreatePersonalFileUploadUrlHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Personal upload URL handler should assign the current user when creation is permitted")]
    [InlineData("file-manager.personal-files.create.own", false)]
    [InlineData("file-manager.personal-files.create.own", true)]
    [InlineData("file-manager.personal-files.create.all", false)]
    [InlineData("file-manager.personal-files.create.all", true)]
    public async Task HandleAsync_Should_AssignCurrentUser_When_CreationIsPermitted(string permission, bool inFolder)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        _user.HasPermission(permission).Returns(true);
        var folder = Folder.Create(
            FolderName.Create("Folder").Value, FileType.Personal, UserId.Create(_userId).Value).Value;
        _folders.GetByIdAsync(folder.Id, cancellationToken).Returns(folder);
        var upload = new PresignedUploadUrl(
            Url: "https://storage.example/key?signature=original",
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
            RequiredHeaders: new Dictionary<string, string> { ["Content-Type"] = "text/plain" });
        _storage.GetPresignedUploadUrlAsync(Arg.Any<string>(), "text/plain", 4, cancellationToken).Returns(upload);
        File? saved = null;
        _files.When(repository => repository.Add(Arg.Any<File>())).Do(call => saved = call.Arg<File>());

        var result = await CreateHandler().HandleAsync(
            new CreatePersonalFileUploadUrlCommand(" File.txt ", "TEXT/PLAIN", 4, inFolder ? folder.Id.Value : null),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        saved.ShouldNotBeNull();
        result.Value.Id.ShouldBe(saved.Id.Value);
        result.Value.Upload.ShouldBeSameAs(upload);
        saved.Name.Value.ShouldBe("File.txt");
        saved.Type.ShouldBe(FileType.Personal);
        saved.UserId.ShouldBe(UserId.Create(_userId).Value);
        saved.FolderId.ShouldBe(inFolder ? folder.Id : null);
        saved.Status.ShouldBe(FileStatus.PendingUpload);
        saved.ContentType.ShouldBeNull();
        saved.Size.ShouldBeNull();
        await _storage.Received(1).GetPresignedUploadUrlAsync(
            saved.StorageKey.Value, "text/plain", 4, cancellationToken);
        await _hierarchyLock.Received(inFolder ? 1 : 0).AcquireAsync(cancellationToken);
        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(default!, default!, default!, cancellationToken);
    }

    [Theory(DisplayName = "Personal upload URL handler should reject authentication when user id is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RejectAuthentication_When_UserIdIsUnavailable(bool emptyUserId)
    {
        _user.UserId.Returns(emptyUserId ? Guid.Empty : null);
        _user.HasPermission(Arg.Any<string>()).Returns(true);

        var result = await CreateHandler().HandleAsync(
            new CreatePersonalFileUploadUrlCommand("file.txt", "text/plain", 4), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        _files.ReceivedCalls().ShouldBeEmpty();
        _storage.ReceivedCalls().ShouldBeEmpty();
        _hierarchyLock.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Personal upload URL handler should deny creation when create permission is missing")]
    [InlineData("")]
    [InlineData("file-manager.personal-files.read.own")]
    [InlineData("file-manager.avatars.create")]
    public async Task HandleAsync_Should_DenyCreation_When_CreatePermissionIsMissing(string permission)
    {
        _user.HasPermission(permission).Returns(true);

        var result = await CreateHandler().HandleAsync(
            new CreatePersonalFileUploadUrlCommand("file.txt", "text/plain", 4), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to create the file is denied.");
        _files.DidNotReceiveWithAnyArgs().Add(default!);
        _storage.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Personal upload URL handler should reject destination when folder cannot contain the file")]
    [InlineData("missing", ErrorType.NotFound)]
    [InlineData("foreign", ErrorType.NotFound)]
    [InlineData("avatar", ErrorType.NotFound)]
    [InlineData("foreign-all", ErrorType.Validation)]
    public async Task HandleAsync_Should_RejectDestination_When_FolderCannotContainTheFile(
        string scenario, ErrorType expected)
    {
        _user.HasPermission("file-manager.personal-files.create.own").Returns(true);
        _user.HasPermission("file-manager.personal-files.create.all").Returns(scenario == "foreign-all");
        var type = scenario == "avatar" ? FileType.Avatar : FileType.Personal;
        var folder = Folder.Create(
            name: FolderName.Create("Folder").Value,
            allowedFileType: type,
            userId: type == FileType.Personal ? UserId.Create(Guid.NewGuid()).Value : null).Value;
        if (scenario != "missing")
        {
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        }

        var result = await CreateHandler().HandleAsync(
            new CreatePersonalFileUploadUrlCommand("file.txt", "text/plain", 4, folder.Id.Value),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Type == expected);
        _files.DidNotReceiveWithAnyArgs().Add(default!);
        _storage.ReceivedCalls().ShouldBeEmpty();
    }

    private CreatePersonalFileUploadUrlHandler CreateHandler()
    {
        var policy = new FileAccessPolicy(_user);
        var service = new FileCreationService(
            filesRepository: _files,
            folderAccessService: new FolderAccessService(
                _folders, _hierarchyLock, new FolderAccessPolicy(_user, policy)),
            fileStorage: _storage,
            unitOfWork: Substitute.For<IUnitOfWork>(),
            fileAccessPolicy: policy);
        return new CreatePersonalFileUploadUrlHandler(service, _user);
    }
}
