using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Update;

public sealed class UpdateUserHandlerTests
{
    [Fact(DisplayName = "Update user handler should delegate update to the write repository when repository succeeds")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IUsersWriteRepository>();
        repository.UpdateAsync(
            Arg.Is<User>(user => user.Id.Value == userId && user.Email.Value == "hero@example.com" && user.ConfirmationStatus.IsConfirmed && user.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            Arg.Is<UserName>(name => name.Value == "hero"),
            UserStatus.Blocked,
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
            Arg.Is<User>(user => user.Id.Value == userId && user.Email.Value == "hero@example.com" && user.ConfirmationStatus.IsConfirmed && user.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            Arg.Is<UserName>(name => name.Value == "hero"),
            UserStatus.Blocked,
            cancellationToken);
    }
}
