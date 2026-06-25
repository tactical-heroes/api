using Organization.Product.Module.Application.Users.Abstractions;
using Organization.Product.Module.Application.Users.GetDetails;

namespace Organization.Product.Module.UnitTests.Application.Users.GetDetails;

public sealed class GetUserDetailsHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return user details when user exists")]
    public async Task HandleAsync_Should_Return_User_Details_When_User_Exists()
    {
        var cancellationToken = CancellationToken.None;
        var id = Guid.NewGuid();
        var readModel = new UserDetailsReadModel(
            Id: id,
            Name: "John Doe",
            Email: "john@example.com",
            Phone: "+12345678901",
            BirthDate: new DateOnly(1990, 1, 1),
            Avatar: "https://example.com/avatar.png");
        var usersReadRepository = Substitute.For<IUsersReadRepository>();
        usersReadRepository
            .GetByIdAsync(id, cancellationToken)
            .Returns(Task.FromResult<UserDetailsReadModel?>(readModel));
        var handler = new GetUserDetailsHandler(usersReadRepository);
        var query = new GetUserDetailsQuery(Id: id);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        result.Value.Id.ShouldBe(id);
        result.Value.Name.ShouldBe("John Doe");
        result.Value.Email.ShouldBe("john@example.com");
        result.Value.Phone.ShouldBe("+12345678901");
        result.Value.BirthDate.ShouldBe(new DateOnly(1990, 1, 1));
        result.Value.Avatar.ShouldBe("https://example.com/avatar.png");
    }

    [Fact(DisplayName = "HandleAsync should return not found when user does not exist")]
    public async Task HandleAsync_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        var cancellationToken = CancellationToken.None;
        var id = Guid.NewGuid();
        var usersReadRepository = Substitute.For<IUsersReadRepository>();
        usersReadRepository
            .GetByIdAsync(id, cancellationToken)
            .Returns(Task.FromResult<UserDetailsReadModel?>(null));
        var handler = new GetUserDetailsHandler(usersReadRepository);
        var query = new GetUserDetailsQuery(Id: id);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            $"User with id '{id}' was not found.");
    }
}
