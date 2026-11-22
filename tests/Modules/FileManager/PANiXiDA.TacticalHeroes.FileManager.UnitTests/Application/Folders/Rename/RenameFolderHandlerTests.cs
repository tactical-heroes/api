using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Rename;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Rename;

public sealed class RenameFolderHandlerTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public RenameFolderHandlerTests()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Rename folder handler should normalize the name when update is permitted")]
    [InlineData("Personal", "file-manager.personal-files.update.own")]
    [InlineData("Avatar", "file-manager.avatars.update")]
    public async Task HandleAsync_Should_NormalizeName_When_UpdateIsPermitted(string type, string permission)
    {
        var folder = CreateFolder(type);
        _currentUser.HasPermission(permission).Returns(true);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var result = await CreateHandler().HandleAsync(
            new RenameFolderCommand(folder.Id.Value, " Heroes "), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        folder.Name.Value.ShouldBe("Heroes");
        _folders.Received(1).Update(folder);
    }

    [Theory(DisplayName = "Rename folder handler should conceal the folder when update is denied")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_ConcealFolder_When_UpdateIsDenied(string type)
    {
        var folder = CreateFolder(type);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var result = await CreateHandler().HandleAsync(
            new RenameFolderCommand(folder.Id.Value, "Heroes"), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        folder.Name.Value.ShouldBe("Documents");
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Rename folder handler should return not found when folder is missing")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FolderIsMissing()
    {
        var command = new RenameFolderCommand(Guid.NewGuid(), "Heroes");

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Rename folder handler should reject input when id is empty")]
    public async Task HandleAsync_Should_RejectInput_When_IdIsEmpty()
    {
        var command = new RenameFolderCommand(Guid.Empty, "Heroes");

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        _folders.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Rename folder handler should preserve metadata when name is invalid")]
    [InlineData("")]
    [InlineData("../folder")]
    [InlineData("folder\\child")]
    public async Task HandleAsync_Should_PreserveMetadata_When_NameIsInvalid(string name)
    {
        var folder = CreateFolder("Personal");
        _currentUser.HasPermission("file-manager.personal-files.update.own").Returns(true);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var result = await CreateHandler().HandleAsync(
            new RenameFolderCommand(folder.Id.Value, name), TestContext.Current.CancellationToken);

        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        result.IsFailure.ShouldBeTrue();
        folder.Name.Value.ShouldBe("Documents");
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    private Folder CreateFolder(string type)
    {
        return Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(_userId).Value : null).Value;
    }

    private RenameFolderHandler CreateHandler()
    {
        return new RenameFolderHandler(
            _folders, new FolderAccessPolicy(_currentUser, new FileAccessPolicy(_currentUser)));
    }
}
