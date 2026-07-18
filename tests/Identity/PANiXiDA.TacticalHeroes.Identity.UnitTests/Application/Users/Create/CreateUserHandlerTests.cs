using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Create;

public sealed class CreateUserHandlerTests
{
    [Fact(DisplayName = "Create user handler should delegate creation to the write repository")]
    public async Task HandleAsync_Should_ReturnUserId_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.read")];
        var repository = Substitute.For<IUsersWriteRepository>();
        repository.AddAsync(
                "hero@example.com",
                "hero",
                "StrongPassword1!",
                true,
                claims,
                "Active",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success(userId));
        var handler = new CreateUserHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateUserCommand(
                "hero@example.com",
                "hero",
                "StrongPassword1!",
                true,
                claims,
                "Active"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(userId);
        await repository.Received(1).AddAsync(
            "hero@example.com",
            "hero",
            "StrongPassword1!",
            true,
            claims,
            "Active",
            cancellationToken);
    }
}
