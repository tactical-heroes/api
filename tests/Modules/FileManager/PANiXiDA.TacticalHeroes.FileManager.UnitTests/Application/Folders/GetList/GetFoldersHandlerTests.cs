using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.GetList;

public sealed class GetFoldersHandlerTests
{
    [Theory(DisplayName = "Folder list handler should pass policy access to the repository when reading is allowed")]
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
        var repository = Substitute.For<IFoldersReadRepository>();
        var page = PaginationResult<FolderListItemReadModel>.Empty(2, 10);
        var filter = new FoldersFilter(Search: "file", AllowedFileType: type);
        var pagination = new PaginationParameters(2, 10);
        var sorting = SortingParameters.Descending("Name");
        var expectedFilter = filter with
        {
            AllowedFileType = type,
            UserId = own ? userId : null
        };
        repository.GetPageAsync(
            Arg.Any<FoldersFilter>(), pagination, sorting, cancellationToken)
            .Returns(page);
        var handler = new GetFoldersHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(new GetFoldersQuery(filter, pagination, sorting), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(page);
        await repository.Received(1).GetPageAsync(
            Arg.Is<FoldersFilter>(actual => actual.AllowedFileType == expectedFilter.AllowedFileType
                && actual.Search == expectedFilter.Search
                && actual.UserId == expectedFilter.UserId),
            pagination,
            sorting,
            cancellationToken);
    }

    [Theory(DisplayName = "Folder list handler should avoid the repository when requested type is forbidden")]
    [InlineData("Personal", "")]
    [InlineData(null, "")]
    [InlineData("Personal", "file-manager.avatars.read")]
    [InlineData("Personal", "file-manager.personal-files.create.all")]
    [InlineData("Avatar", "file-manager.personal-files.read.all")]
    public async Task HandleAsync_Should_AvoidRepository_When_RequestedTypeIsForbidden(string? type, string permission)
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        currentUser.HasPermission(permission).Returns(true);
        var repository = Substitute.For<IFoldersReadRepository>();
        var handler = new GetFoldersHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(
            new GetFoldersQuery(
                new FoldersFilter(AllowedFileType: type),
                new PaginationParameters(),
                SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to folders is denied.");
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder list handler should reject the caller when user id is absent")]
    public async Task HandleAsync_Should_RejectCaller_When_UserIdIsAbsent()
    {
        var currentUser = CreateCurrentUser(null);
        var repository = Substitute.For<IFoldersReadRepository>();
        var handler = new GetFoldersHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(
            new GetFoldersQuery(
                new FoldersFilter(AllowedFileType: "Personal"),
                new PaginationParameters(),
                SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder list handler should reject the type when type is unknown")]
    public async Task HandleAsync_Should_RejectType_When_TypeIsUnknown()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        var repository = Substitute.For<IFoldersReadRepository>();
        var handler = new GetFoldersHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(
            new GetFoldersQuery(
                new FoldersFilter(AllowedFileType: "Unknown"),
                new PaginationParameters(),
                SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to folders is denied.");
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Folder list handler should require both file types when type is omitted")]
    [InlineData("file-manager.personal-files.read.all,file-manager.avatars.read", false, true)]
    [InlineData("file-manager.personal-files.read.own,file-manager.avatars.read", true, true)]
    [InlineData("file-manager.personal-files.read.all", false, false)]
    [InlineData("file-manager.personal-files.read.own", true, false)]
    [InlineData("file-manager.avatars.read", false, false)]
    public async Task HandleAsync_Should_RequireBothFileTypes_When_TypeIsOmitted(
        string permissions, bool ownPersonal, bool allowed)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        foreach (var permission in permissions.Split(','))
        {
            currentUser.HasPermission(permission).Returns(true);
        }
        var repository = Substitute.For<IFoldersReadRepository>();
        var filter = new FoldersFilter(Search: "report");
        var pagination = new PaginationParameters();
        var page = PaginationResult<FolderListItemReadModel>.Empty(1, 10);
        repository.GetPageAsync(Arg.Any<FoldersFilter>(), pagination, SortingParameters.None, cancellationToken)
            .Returns(page);
        var handler = new GetFoldersHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(
            new GetFoldersQuery(filter, pagination, SortingParameters.None),
            cancellationToken);

        result.IsSuccess.ShouldBe(allowed);
        if (!allowed)
        {
            result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to folders is denied.");
            repository.ReceivedCalls().ShouldBeEmpty();
            return;
        }

        result.Value.ShouldBeSameAs(page);
        await repository.Received(1)
            .GetPageAsync(
                Arg.Is<FoldersFilter>(actual => actual.AllowedFileType == null
                    && actual.Search == filter.Search
                    && actual.UserId == (ownPersonal
                        ? userId
                        : null)),
                pagination,
                SortingParameters.None,
                cancellationToken);
    }

    [Theory(DisplayName = "Folder list handler should enforce the requested owner when owner and permissions vary")]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    public async Task HandleAsync_Should_EnforceRequestedOwner_When_OwnerAndPermissionsVary(
        bool all, bool own, bool allowed)
    {
        var userId = Guid.NewGuid();
        var ownerId = own ? userId : Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission("file-manager.personal-files.read.all").Returns(all);
        currentUser.HasPermission("file-manager.personal-files.read.own").Returns(true);
        currentUser.HasPermission("file-manager.avatars.read").Returns(true);
        var repository = Substitute.For<IFoldersReadRepository>();
        repository.GetPageAsync(
            Arg.Any<FoldersFilter>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<SortingParameters>(),
            Arg.Any<CancellationToken>())
            .Returns(PaginationResult<FolderListItemReadModel>.Empty(1, 10));
        var handler = new GetFoldersHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(
            new GetFoldersQuery(new FoldersFilter(UserId: ownerId), new PaginationParameters(), SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBe(allowed);
        if (allowed)
        {
            await repository.Received(1).GetPageAsync(
                Arg.Is<FoldersFilter>(filter => filter.AllowedFileType == null && filter.UserId == ownerId),
                Arg.Any<PaginationParameters>(),
                Arg.Any<SortingParameters>(),
                Arg.Any<CancellationToken>());
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to folders is denied.");
            repository.ReceivedCalls().ShouldBeEmpty();
        }
    }

    [Fact(DisplayName = "Folder list handler should reject caller when authentication is absent")]
    public async Task HandleAsync_Should_RejectCaller_When_AuthenticationIsAbsent()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        currentUser.IsAuthenticated.Returns(false);
        var repository = Substitute.For<IFoldersReadRepository>();
        var handler = new GetFoldersHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(
            new GetFoldersQuery(new FoldersFilter(), new PaginationParameters(), SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
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
