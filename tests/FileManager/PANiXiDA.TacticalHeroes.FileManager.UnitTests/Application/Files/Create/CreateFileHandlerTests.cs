using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Create;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Create;

public sealed class CreateFileHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IFolderHierarchyLock _hierarchyLock = Substitute.For<IFolderHierarchyLock>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public CreateFileHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(_userId);
        _user.HasPermission("file-manager.personal-files.create.all").Returns(true);
        _user.HasPermission("file-manager.avatars.create").Returns(true);
        _unitOfWork.ExecuteInTransactionAsync(
            Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>()(call.Arg<CancellationToken>()));
    }

    [Theory(DisplayName = "Create file handler should upload the requested scope when creation is permitted")]
    [InlineData("Personal", false, false)]
    [InlineData("Personal", true, true)]
    [InlineData("Avatar", false, false)]
    [InlineData("Avatar", false, true)]
    public async Task HandleAsync_Should_UploadRequestedScope_When_CreationIsPermitted(
        string type, bool foreignOwner, bool inFolder)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : _userId : null;
        var folder = Folder.Create(
            FolderName.Create("Folder").Value, FileType.FromName(type), UserId.Create(ownerId).Value).Value;
        _folders.GetByIdAsync(folder.Id, cancellationToken).Returns(folder);
        await using var content = new MemoryStream([1, 2, 3]);
        File? saved = null;
        _files.When(repository => repository.Add(Arg.Any<File>())).Do(call => saved = call.Arg<File>());

        var result = await CreateHandler().HandleAsync(
            new CreateFileCommand(
                " File.txt ", type, "TEXT/PLAIN", content.Length, content, ownerId, inFolder ? folder.Id.Value : null),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        saved.ShouldNotBeNull();
        saved.Id.Value.ShouldBe(result.Value);
        saved.Name.Value.ShouldBe("File.txt");
        saved.Type.ShouldBe(FileType.FromName(type));
        (saved.UserId?.Value).ShouldBe(ownerId);
        (saved.FolderId?.Value).ShouldBe(inFolder ? folder.Id.Value : null);
        saved.Status.ShouldBe(FileStatus.Ready);
        saved.Size!.Value.ShouldBe(content.Length);
        saved.ContentType!.Value.ShouldBe("text/plain");
        await _storage.Received(1).UploadAsync(saved.StorageKey.Value, content, "text/plain", cancellationToken);
        await _unitOfWork.Received(2).SaveChangesAsync(cancellationToken);
        await _hierarchyLock.Received(inFolder ? 1 : 0).AcquireAsync(cancellationToken);
        _folders.DidNotReceiveWithAnyArgs().Update(default!);
        content.CanRead.ShouldBeTrue();
    }

    [Theory(DisplayName = "Create file handler should reject creation when metadata ownership or access is invalid")]
    [InlineData("unknown-type", ErrorType.Validation)]
    [InlineData("missing-owner", ErrorType.Validation)]
    [InlineData("avatar-owner", ErrorType.Validation)]
    [InlineData("empty-owner", ErrorType.Validation)]
    [InlineData("foreign-own", ErrorType.Forbidden)]
    [InlineData("missing-permission", ErrorType.Forbidden)]
    public async Task HandleAsync_Should_RejectCreation_When_MetadataOwnershipOrAccessIsInvalid(
        string scenario, ErrorType expected)
    {
        await using var content = new MemoryStream([1]);
        var command = new CreateFileCommand("file.txt", "Personal", "text/plain", content.Length, content, _userId);
        command = scenario switch
        {
            "unknown-type" => command with { Type = "Unknown" },
            "missing-owner" => command with { UserId = null },
            "avatar-owner" => command with { Type = "Avatar" },
            "empty-owner" => command with { UserId = Guid.Empty },
            "foreign-own" => command with { UserId = Guid.NewGuid() },
            _ => command
        };
        if (scenario is "foreign-own" or "missing-permission")
        {
            _user.HasPermission("file-manager.personal-files.create.all").Returns(false);
            _user.HasPermission("file-manager.personal-files.create.own").Returns(scenario == "foreign-own");
        }

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Type == expected);
        _files.ReceivedCalls().ShouldBeEmpty();
        _storage.ReceivedCalls().ShouldBeEmpty();
        _unitOfWork.ReceivedCalls().ShouldBeEmpty();
    }

    private CreateFileHandler CreateHandler()
    {
        var policy = new FileAccessPolicy(_user);
        return new CreateFileHandler(new FileCreationService(
            filesRepository: _files,
            foldersRepository: _folders,
            folderHierarchyLock: _hierarchyLock,
            fileStorage: _storage,
            unitOfWork: _unitOfWork,
            fileAccessPolicy: policy,
            folderAccessPolicy: new FolderAccessPolicy(_user, policy)));
    }
}
