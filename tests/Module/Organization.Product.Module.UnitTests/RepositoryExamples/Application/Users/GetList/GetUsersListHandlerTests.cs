using Organization.Product.Module.Application.Users;
using Organization.Product.Module.Application.Users.Abstractions;
using Organization.Product.Module.Application.Users.GetList;

namespace Organization.Product.Module.UnitTests.Application.Users.GetList;

public sealed class GetUsersListHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return paged list from repository when query is valid")]
    public async Task HandleAsync_Should_Return_Paged_List_From_Repository_When_Query_Is_Valid()
    {
        var cancellationToken = CancellationToken.None;
        var filterParameters = new UsersFilterParameters(Role: "Admin");
        var paginationParameters = new PaginationParameters(PageNumber: 2, PageSize: 10);
        var sortParameters = new SortParameters(Field: "Name", Order: SortOrder.Descending);
        var items = new[]
        {
            new UserListItemReadModel(
                Id: Guid.NewGuid(),
                Name: "John Doe",
                Email: "john@example.com",
                Phone: "+12345678901",
                BirthDate: new DateOnly(1990, 1, 1),
                Avatar: null)
        };
        var pagedResult = PaginationResult<UserListItemReadModel>.Create(
            items,
            paginationParameters.PageNumber,
            paginationParameters.PageSize,
            totalCount: 25);
        var usersReadRepository = Substitute.For<IUsersReadRepository>();
        usersReadRepository
            .GetPagedListAsync(
                filterParameters,
                paginationParameters,
                sortParameters,
                cancellationToken)
            .Returns(Task.FromResult(pagedResult));
        var handler = new GetUsersListHandler(usersReadRepository);
        var query = new GetUsersListQuery(
            FilterParameters: filterParameters,
            PaginationParameters: paginationParameters,
            SortParameters: sortParameters);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(pagedResult);
        filterParameters.Role.ShouldBe("Admin");
        result.Value.Items.ShouldBe(items);
        var item = result.Value.Items.ShouldHaveSingleItem();
        item.Id.ShouldBe(items[0].Id);
        item.Name.ShouldBe("John Doe");
        item.Email.ShouldBe("john@example.com");
        item.Phone.ShouldBe("+12345678901");
        item.BirthDate.ShouldBe(new DateOnly(1990, 1, 1));
        item.Avatar.ShouldBeNull();
        result.Value.TotalCount.ShouldBe(25);
        _ = usersReadRepository.Received(1).GetPagedListAsync(
            filterParameters,
            paginationParameters,
            sortParameters,
            cancellationToken);
    }
}
