using PANiXiDA.TacticalHeroes.Identity.Application.Users.Unblock;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Unblock;

public sealed class UnblockUserHandlerTests
{
    [Fact(DisplayName = "Unblock user handler should delegate unblocking to the write repository when repository succeeds")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        var repository = Substitute.For<IUsersRepository>();
        repository.UnblockAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new UnblockUserHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new UnblockUserCommand(userId), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).UnblockAsync(userId, cancellationToken);
    }
}
