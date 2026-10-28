using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetList;

public sealed class GetFilesHandlerTests
{
    [Theory(DisplayName = "File list handler should pass policy access to the repository when reading is allowed")]
    [InlineData(" Personal ", "file-manager.personal-files.read.own", true)]
    [InlineData("Personal", "file-manager.personal-files.read.all", false)]
    [InlineData("Avatar", "file-manager.avatars.read", false)]
    public async Task HandleAsync_Should_PassPolicyAccessToRepository_When_ReadingIsAllowed(
        string type, string permission, bool own)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(permission).Returns(true);
        var repository = Substitute.For<IFilesReadRepository>();
        var page = PaginationResult<FileListItemReadModel>.Empty(2, 10);
        var filter = new FilesFilter(type, "file");
        var pagination = new PaginationParameters(2, 10);
        var sorting = SortingParameters.Descending("Name");
        var expectedAccess = new FileReadAccess(own ? userId : null);
        repository.GetPageAsync(
            filter with { Type = type.Trim() }, expectedAccess, pagination, sorting, cancellationToken)
            .Returns(page);
        var handler = new GetFilesHandler(repository, currentUser, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(new GetFilesQuery(filter, pagination, sorting), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(page);
        await repository.Received(1).GetPageAsync(
            filter with { Type = type.Trim() }, expectedAccess, pagination, sorting, cancellationToken);
    }

    [Theory(DisplayName = "File list handler should avoid the repository when requested type is forbidden")]
    [InlineData("Personal", "")]
    [InlineData("Personal", "file-manager.avatars.read")]
    [InlineData("Personal", "file-manager.personal-files.create.all")]
    [InlineData("Avatar", "file-manager.personal-files.read.all")]
    public async Task HandleAsync_Should_AvoidRepository_When_RequestedTypeIsForbidden(string type, string permission)
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        currentUser.HasPermission(permission).Returns(true);
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetFilesHandler(repository, currentUser, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFilesQuery(new FilesFilter(type), new PaginationParameters(), SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to this file type is denied.");
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "File list handler should reject the caller when user id is absent")]
    public async Task HandleAsync_Should_RejectCaller_When_UserIdIsAbsent()
    {
        var currentUser = CreateCurrentUser(null);
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetFilesHandler(repository, currentUser, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFilesQuery(new FilesFilter("Personal"), new PaginationParameters(), SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "File list handler should reject the type when type is unknown")]
    public async Task HandleAsync_Should_RejectType_When_TypeIsUnknown()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetFilesHandler(repository, currentUser, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFilesQuery(new FilesFilter("Unknown"), new PaginationParameters(), SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    private static ICurrentUser CreateCurrentUser(Guid? userId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(userId);
        return currentUser;
    }
}
