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
        var expectedFilter = filter with
        {
            Type = type.Trim(),
            UserId = own ? userId : null
        };
        repository.GetPageAsync(
            Arg.Any<FilesFilter>(), pagination, sorting, cancellationToken)
            .Returns(page);
        var handler = new GetFilesHandler(repository, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(new GetFilesQuery(filter, pagination, sorting), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(page);
        await repository.Received(1).GetPageAsync(
            Arg.Is<FilesFilter>(actual => actual.Type == expectedFilter.Type
                && actual.Search == expectedFilter.Search
                && actual.UserId == expectedFilter.UserId),
            pagination,
            sorting,
            cancellationToken);
    }

    [Theory(DisplayName = "File list handler should avoid the repository when requested type is forbidden")]
    [InlineData("Personal", "")]
    [InlineData(null, "")]
    [InlineData("Personal", "file-manager.avatars.read")]
    [InlineData("Personal", "file-manager.personal-files.create.all")]
    [InlineData("Avatar", "file-manager.personal-files.read.all")]
    public async Task HandleAsync_Should_AvoidRepository_When_RequestedTypeIsForbidden(string? type, string permission)
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        currentUser.HasPermission(permission).Returns(true);
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetFilesHandler(repository, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFilesQuery(new FilesFilter(type), new PaginationParameters(), SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to files is denied.");
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "File list handler should reject the caller when user id is absent")]
    public async Task HandleAsync_Should_RejectCaller_When_UserIdIsAbsent()
    {
        var currentUser = CreateCurrentUser(null);
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetFilesHandler(repository, new FileAccessPolicy(currentUser));

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
        var handler = new GetFilesHandler(repository, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFilesQuery(new FilesFilter("Unknown"), new PaginationParameters(), SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "File list handler should normalize type when type is omitted")]
    [InlineData("file-manager.personal-files.read.all,file-manager.avatars.read", false, null)]
    [InlineData("file-manager.personal-files.read.own,file-manager.avatars.read", true, null)]
    [InlineData("file-manager.personal-files.read.all", false, "Personal")]
    [InlineData("file-manager.personal-files.read.own", true, "Personal")]
    [InlineData("file-manager.avatars.read", false, "Avatar")]
    public async Task HandleAsync_Should_NormalizeType_When_TypeIsOmitted(
        string permissions, bool ownPersonal, string? expectedType)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        foreach (var permission in permissions.Split(','))
        {
            currentUser.HasPermission(permission).Returns(true);
        }
        var repository = Substitute.For<IFilesReadRepository>();
        var filter = new FilesFilter(Search: "report");
        var pagination = new PaginationParameters();
        var page = PaginationResult<FileListItemReadModel>.Empty(1, 10);
        repository.GetPageAsync(Arg.Any<FilesFilter>(), pagination, SortingParameters.None, cancellationToken)
            .Returns(page);
        var handler = new GetFilesHandler(repository, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFilesQuery(filter, pagination, SortingParameters.None),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(page);
        await repository.Received(1)
            .GetPageAsync(
                Arg.Is<FilesFilter>(actual => actual.Type == expectedType
                    && actual.Search == filter.Search
                    && actual.UserId == (ownPersonal
                        ? userId
                        : null)),
                pagination,
                SortingParameters.None,
                cancellationToken);
    }

    [Theory(DisplayName = "File list handler should enforce the requested owner when owner and permissions vary")]
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
        var repository = Substitute.For<IFilesReadRepository>();
        repository.GetPageAsync(
            Arg.Any<FilesFilter>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<SortingParameters>(),
            Arg.Any<CancellationToken>())
            .Returns(PaginationResult<FileListItemReadModel>.Empty(1, 10));
        var handler = new GetFilesHandler(repository, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFilesQuery(new FilesFilter(UserId: ownerId), new PaginationParameters(), SortingParameters.None),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBe(allowed);
        if (allowed)
        {
            await repository.Received(1).GetPageAsync(
                Arg.Is<FilesFilter>(filter => filter.Type == null && filter.UserId == ownerId),
                Arg.Any<PaginationParameters>(),
                Arg.Any<SortingParameters>(),
                Arg.Any<CancellationToken>());
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to files is denied.");
            repository.ReceivedCalls().ShouldBeEmpty();
        }
    }

    [Fact(DisplayName = "File list handler should reject caller when authentication is absent")]
    public async Task HandleAsync_Should_RejectCaller_When_AuthenticationIsAbsent()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        currentUser.IsAuthenticated.Returns(false);
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetFilesHandler(repository, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFilesQuery(new FilesFilter(), new PaginationParameters(), SortingParameters.None),
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
