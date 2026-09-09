using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
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
        var repository = Substitute.For<IUsersRepository>();
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

    [Theory(DisplayName = "Update user handler should reject invalid values without persistence when command is invalid")]
    [InlineData("invalid-email", "hero", "Active", "permission", "heroes.read")]
    [InlineData("hero@example.com", "", "Active", "permission", "heroes.read")]
    [InlineData("hero@example.com", "hero", "Unknown", "permission", "heroes.read")]
    [InlineData("hero@example.com", "hero", "Active", "", "")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutPersisting_When_CommandIsInvalid(
        string email,
        string userName,
        string status,
        string claimType,
        string claimValue)
    {
        var repository = Substitute.For<IUsersRepository>();
        var handler = new UpdateUserHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateUserCommand(Guid.CreateVersion7(), email, userName, false, [new Claim(claimType, claimValue)], status),
            cancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        await repository.DidNotReceiveWithAnyArgs().UpdateAsync(null!, null!, null!, cancellationToken);
    }
}
