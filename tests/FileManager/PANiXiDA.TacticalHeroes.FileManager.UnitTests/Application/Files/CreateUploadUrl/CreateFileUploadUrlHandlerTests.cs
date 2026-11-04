using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreateUploadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
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

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CreateUploadUrl;

public sealed class CreateFileUploadUrlHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IFolderHierarchyLock _hierarchyLock = Substitute.For<IFolderHierarchyLock>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public CreateFileUploadUrlHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(_userId);
        _user.HasPermission("file-manager.personal-files.create.own").Returns(true);
        _user.HasPermission("file-manager.avatars.create").Returns(true);
    }

    [Theory(DisplayName = "Upload URL handler should reserve a pending file when metadata and access are valid")]
    [InlineData("Personal", false)]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    [InlineData("Avatar", true)]
    public async Task HandleAsync_Should_ReservePendingFile_When_MetadataAndAccessAreValid(string type, bool inFolder)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var ownerId = type == "Personal" ? _userId : (Guid?)null;
        var folder = Folder.Create(
            FolderName.Create("Folder").Value, FileType.FromName(type), UserId.Create(ownerId).Value).Value;
        _folders.GetByIdAsync(folder.Id, cancellationToken).Returns(folder);
        var upload = new PresignedUploadUrl(
            Url: "https://storage.example/key?signature=original",
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
            RequiredHeaders: new Dictionary<string, string> { ["Content-Type"] = "text/plain" });
        _storage.GetPresignedUploadUrlAsync(Arg.Any<string>(), "text/plain", 4, cancellationToken).Returns(upload);
        File? saved = null;
        _files.When(repository => repository.Add(Arg.Any<File>())).Do(call => saved = call.Arg<File>());

        var result = await CreateHandler().HandleAsync(
            new CreateFileUploadUrlCommand(
                " File.txt ", type, "TEXT/PLAIN", 4, ownerId, inFolder ? folder.Id.Value : null),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        saved.ShouldNotBeNull();
        result.Value.Id.ShouldBe(saved.Id.Value);
        result.Value.Upload.ShouldBeSameAs(upload);
        saved.Name.Value.ShouldBe("File.txt");
        saved.Type.ShouldBe(FileType.FromName(type));
        (saved.UserId?.Value).ShouldBe(ownerId);
        saved.Status.ShouldBe(FileStatus.PendingUpload);
        saved.ContentType.ShouldBeNull();
        saved.Size.ShouldBeNull();
        (saved.FolderId?.Value).ShouldBe(inFolder ? folder.Id.Value : null);
        await _storage.Received(1).GetPresignedUploadUrlAsync(
            saved.StorageKey.Value, "text/plain", 4, cancellationToken);
        await _hierarchyLock.Received(inFolder ? 1 : 0).AcquireAsync(cancellationToken);
        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(default!, default!, default!, cancellationToken);
        _folders.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Upload URL handler should reject creation when metadata ownership or access is invalid")]
    [InlineData("name", ErrorType.Validation)]
    [InlineData("type", ErrorType.Validation)]
    [InlineData("size", ErrorType.Validation)]
    [InlineData("content-type", ErrorType.Validation)]
    [InlineData("user-id", ErrorType.Validation)]
    [InlineData("folder-id", ErrorType.Validation)]
    [InlineData("missing-owner", ErrorType.Validation)]
    [InlineData("avatar-owner", ErrorType.Validation)]
    [InlineData("foreign-owner", ErrorType.Forbidden)]
    [InlineData("no-permission", ErrorType.Forbidden)]
    [InlineData("anonymous", ErrorType.Forbidden)]
    public async Task HandleAsync_Should_RejectCreation_When_MetadataOwnershipOrAccessIsInvalid(
        string scenario, ErrorType expected)
    {
        var command = new CreateFileUploadUrlCommand("file.txt", "Personal", "text/plain", 4, _userId);
        command = scenario switch
        {
            "name" => command with { Name = "../file.txt" },
            "type" => command with { Type = "Unknown" },
            "size" => command with { Size = 0 },
            "content-type" => command with { ContentType = "text/*" },
            "user-id" => command with { UserId = Guid.Empty },
            "folder-id" => command with { FolderId = Guid.Empty },
            "missing-owner" => command with { UserId = null },
            "avatar-owner" => command with { Type = "Avatar" },
            "foreign-owner" => command with { UserId = Guid.NewGuid() },
            _ => command
        };
        if (scenario == "no-permission")
        {
            _user.HasPermission(Arg.Any<string>()).Returns(false);
        }
        _user.IsAuthenticated.Returns(scenario != "anonymous");

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Type == expected);
        _files.DidNotReceiveWithAnyArgs().Add(default!);
        await _storage.DidNotReceiveWithAnyArgs().GetPresignedUploadUrlAsync(
            default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Upload URL handler should reject destination when folder is unavailable or incompatible")]
    [InlineData("missing", ErrorType.NotFound)]
    [InlineData("foreign", ErrorType.NotFound)]
    [InlineData("incompatible", ErrorType.Validation)]
    public async Task HandleAsync_Should_RejectDestination_When_FolderIsUnavailableOrIncompatible(
        string scenario, ErrorType expected)
    {
        var type = scenario == "incompatible" ? FileType.Avatar : FileType.Personal;
        var ownerId = scenario == "foreign" ? Guid.NewGuid() : _userId;
        var folder = Folder.Create(
            FolderName.Create("Folder").Value, type, type == FileType.Personal ? UserId.Create(ownerId).Value : null)
            .Value;
        if (scenario != "missing")
        {
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        }

        var result = await CreateHandler().HandleAsync(
            new CreateFileUploadUrlCommand("file.txt", "Personal", "text/plain", 4, _userId, folder.Id.Value),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldContain(error => error.Type == expected);
        _files.DidNotReceiveWithAnyArgs().Add(default!);
        await _storage.DidNotReceiveWithAnyArgs().GetPresignedUploadUrlAsync(
            default!, default!, default, TestContext.Current.CancellationToken);
        Received.InOrder(() =>
        {
            _hierarchyLock.AcquireAsync(TestContext.Current.CancellationToken);
            _folders.GetByIdAsync(folder.Id, TestContext.Current.CancellationToken);
        });
    }

    [Fact(DisplayName = "Upload URL handler should preserve storage failures when signing fails")]
    public async Task HandleAsync_Should_PreserveStorageFailures_When_SigningFails()
    {
        var failure = new InvalidOperationException("Signing failed");
        _storage.GetPresignedUploadUrlAsync(Arg.Any<string>(), Arg.Any<string>(), 4, Arg.Any<CancellationToken>())
            .ThrowsAsync(failure);

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() => CreateHandler().HandleAsync(
            new CreateFileUploadUrlCommand("file.txt", "Personal", "text/plain", 4, _userId),
            TestContext.Current.CancellationToken));

        thrown.ShouldBeSameAs(failure);
        _files.DidNotReceiveWithAnyArgs().Add(default!);
    }

    private CreateFileUploadUrlHandler CreateHandler()
    {
        var policy = new FileAccessPolicy(_user);
        return new CreateFileUploadUrlHandler(
            _files, _folders, _hierarchyLock, _storage, policy, new FolderAccessPolicy(_user, policy));
    }
}
