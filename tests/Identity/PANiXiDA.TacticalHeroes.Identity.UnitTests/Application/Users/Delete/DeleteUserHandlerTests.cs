using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Delete;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Delete;

public sealed class DeleteUserHandlerTests
{
    [Fact(DisplayName = "Delete user handler should delegate deletion to the write repository")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        var repository = Substitute.For<IUsersWriteRepository>();
        repository.DeleteAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new DeleteUserHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new DeleteUserCommand(userId), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).DeleteAsync(userId, cancellationToken);
    }
}
