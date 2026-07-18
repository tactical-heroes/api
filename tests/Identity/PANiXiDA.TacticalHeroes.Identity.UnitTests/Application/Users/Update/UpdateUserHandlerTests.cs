using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Update;

public sealed class UpdateUserHandlerTests
{
    [Fact(DisplayName = "Update user handler should delegate update to the write repository")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IUsersWriteRepository>();
        repository.UpdateAsync(
                userId,
                "hero@example.com",
                "hero",
                true,
                claims,
                "Blocked",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new UpdateUserHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateUserCommand(
                userId,
                "hero@example.com",
                "hero",
                true,
                claims,
                "Blocked"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(
            userId,
            "hero@example.com",
            "hero",
            true,
            claims,
            "Blocked",
            cancellationToken);
    }
}
