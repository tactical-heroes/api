using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Create;

public sealed class CreateUserHandlerTests
{
    [Fact(DisplayName = "Create user handler should delegate creation to the write repository when repository succeeds")]
    public async Task HandleAsync_Should_ReturnUserId_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.read")];
        var repository = Substitute.For<IUsersRepository>();
        repository.AddAsync(
            Arg.Is<User>(user => user.Email.Value == "hero@example.com" && user.UserName.Value == "hero" && user.Status == UserStatus.Active && user.ConfirmationStatus.IsConfirmed && user.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.read")),
            "StrongPassword1!",
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
            Arg.Is<User>(user => user.Email.Value == "hero@example.com" && user.UserName.Value == "hero" && user.Status == UserStatus.Active && user.ConfirmationStatus.IsConfirmed && user.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.read")),
            "StrongPassword1!",
            cancellationToken);
    }

    [Theory(DisplayName = "Create user handler should reject invalid values without persistence when command is invalid")]
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
        var handler = new CreateUserHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateUserCommand(email, userName, "StrongPassword1!", false, [new Claim(claimType, claimValue)], status),
            cancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        await repository.DidNotReceiveWithAnyArgs().AddAsync(null!, string.Empty, cancellationToken);
    }
}
