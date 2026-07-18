using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Block;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Block;

public sealed class BlockUserHandlerTests
{
    [Fact(DisplayName = "Block user handler should delegate blocking to the write repository")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        var repository = Substitute.For<IUsersWriteRepository>();
        repository.BlockAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new BlockUserHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new BlockUserCommand(userId), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).BlockAsync(userId, cancellationToken);
    }
}
