using Organization.Product.Module.Application.Users.Delete;
using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Abstractions;

namespace Organization.Product.Module.UnitTests.Application.Users.Delete;

public sealed class DeleteUserHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return failure when id is empty")]
    public async Task HandleAsync_Should_Return_Failure_When_Id_Is_Empty()
    {
        var usersRepository = Substitute.For<IUsersRepository>();
        var handler = new DeleteUserHandler(usersRepository);
        var command = new DeleteUserCommand(Id: Guid.Empty);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "User id cannot be empty.");
        _ = usersRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        _ = usersRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "HandleAsync should return not found when user does not exist")]
    public async Task HandleAsync_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        var cancellationToken = CancellationToken.None;
        var id = Guid.NewGuid();
        var usersRepository = Substitute.For<IUsersRepository>();
        usersRepository
            .GetByIdAsync(
                Arg.Is<UserId>(userId => userId.Value == id),
                cancellationToken)
            .Returns(Task.FromResult<User?>(null));
        var handler = new DeleteUserHandler(usersRepository);
        var command = new DeleteUserCommand(Id: id);

        var result = await handler.HandleAsync(command, cancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            $"User with id '{id}' was not found.");
        _ = usersRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "HandleAsync should delete user when user exists")]
    public async Task HandleAsync_Should_Delete_User_When_User_Exists()
    {
        var cancellationToken = CancellationToken.None;
        var user = User.Create(
            role: "User",
            name: "John Doe",
            email: "john.doe@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var usersRepository = Substitute.For<IUsersRepository>();
        usersRepository
            .GetByIdAsync(user.Id, cancellationToken)
            .Returns(Task.FromResult<User?>(user));
        usersRepository
            .DeleteAsync(user, cancellationToken)
            .Returns(Task.CompletedTask);
        var handler = new DeleteUserHandler(usersRepository);
        var command = new DeleteUserCommand(Id: user.Id.Value);

        var result = await handler.HandleAsync(command, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _ = usersRepository.Received(1).DeleteAsync(user, cancellationToken);
    }
}
