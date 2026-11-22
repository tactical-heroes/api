using System.Security.Claims;

using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Update;

public sealed class UpdateUserHandlerTests
{
    [Theory(DisplayName = "Update user handler should update the loaded user when repository succeeds")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_UpdateLoadedUser_When_RepositorySucceeds(bool isConfirmed)
    {
        var userId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IUsersRepository>();
        var existingUser = RestoreUser(userId, isConfirmed);
        repository.GetByIdAsync(Arg.Is<UserId>(id => id.Value == userId), Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingUser));
        repository.UpdateAsync(
            user: Arg.Is<User>(user => ReferenceEquals(user, existingUser)
                && user.Id.Value == userId
                && user.Email.Value == "hero@example.com"
                && user.UserName.Value == "hero"
                && user.Status == UserStatus.Blocked
                && user.ConfirmationStatus.IsConfirmed
                && user.GetDomainEvents().Count == (isConfirmed ? 0 : 1)
                && user.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            currentAccessToken: null,
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new UpdateUserHandler(repository, Substitute.For<ICurrentUser>());
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateUserCommand(
                userId,
                "hero@example.com",
                "hero",
                true,
                claims,
                "Blocked",
                RoleIds: []),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).GetByIdAsync(
            Arg.Is<UserId>(id => id.Value == userId), cancellationToken);
        await repository.Received(1).UpdateAsync(
            user: Arg.Is<User>(user => ReferenceEquals(user, existingUser)
                && user.Id.Value == userId
                && user.Email.Value == "hero@example.com"
                && user.UserName.Value == "hero"
                && user.Status == UserStatus.Blocked
                && user.ConfirmationStatus.IsConfirmed
                && user.GetDomainEvents().Count == (isConfirmed ? 0 : 1)
                && user.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            currentAccessToken: null,
            cancellationToken: cancellationToken);
    }

    [Theory(
        DisplayName = "Update user handler should reject invalid values without persistence when command is invalid")]
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
        var handler = new UpdateUserHandler(repository, Substitute.For<ICurrentUser>());
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateUserCommand(
                Guid.CreateVersion7(),
                email,
                userName,
                false,
                [new Claim(claimType, claimValue)],
                status,
                RoleIds: []),
            cancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        repository.ReceivedCalls().ShouldBeEmpty();
        await repository.DidNotReceiveWithAnyArgs().UpdateAsync(
            user: null!,
            currentAccessToken: null,
            cancellationToken: cancellationToken);
    }

    [Fact(DisplayName = "User handler should pass assigned role IDs to persistence when role ids are provided")]
    public async Task HandleAsync_Should_PassRoleIds_When_RoleIdsAreProvided()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();
        var repository = Substitute.For<IUsersRepository>();
        repository.GetByIdAsync(Arg.Is<UserId>(id => id.Value == userId), Arg.Any<CancellationToken>())
            .Returns(Result.Success(RestoreUser(userId)));
        repository.UpdateAsync(
            user: Arg.Any<User>(),
            currentAccessToken: null,
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new UpdateUserHandler(repository, Substitute.For<ICurrentUser>());

        var result = await handler.HandleAsync(
            new UpdateUserCommand(
                Id: userId,
                Email: "hero@example.com",
                UserName: "hero",
                IsConfirmed: true,
                Claims: [],
                Status: "Active",
                RoleIds: [roleId]),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(
            user: Arg.Is<User>(user => user.RoleIds.Single().Value == roleId),
            currentAccessToken: null,
            cancellationToken: cancellationToken);
    }

    [Fact(DisplayName = "User handler should reject an empty role ID when role id is invalid")]
    public async Task HandleAsync_Should_ReturnValidationFailure_When_RoleIdIsInvalid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = Substitute.For<IUsersRepository>();
        var handler = new UpdateUserHandler(repository, Substitute.For<ICurrentUser>());

        var result = await handler.HandleAsync(
            new UpdateUserCommand(
                Id: Guid.CreateVersion7(),
                Email: "hero@example.com",
                UserName: "hero",
                IsConfirmed: true,
                Claims: [],
                Status: "Active",
                RoleIds: [Guid.Empty]),
            cancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "User handler should pass the token only for self updates when user is updated")]
    [InlineData(true, "current-token", "current-token")]
    [InlineData(false, "current-token", null)]
    [InlineData(true, null, null)]
    public async Task HandleAsync_Should_PassTokenOnlyForSelfUpdate_When_UserIsUpdated(
        bool isCurrentUser, string? accessToken, string? expectedToken)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.CreateVersion7();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(isCurrentUser ? userId : Guid.CreateVersion7());
        currentUser.AccessToken.Returns(accessToken);
        var repository = Substitute.For<IUsersRepository>();
        repository.GetByIdAsync(Arg.Is<UserId>(id => id.Value == userId), Arg.Any<CancellationToken>())
            .Returns(Result.Success(RestoreUser(userId)));
        repository.UpdateAsync(Arg.Any<User>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new UpdateUserHandler(repository, currentUser);

        var result = await handler.HandleAsync(
            new UpdateUserCommand(userId, "hero@example.com", "hero", true, [], "Active", []),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(
            Arg.Is<User>(user => user.Id.Value == userId),
            expectedToken,
            cancellationToken);
    }

    [Fact(DisplayName = "Update user handler should not persist when user does not exist")]
    public async Task HandleAsync_Should_ReturnNotFoundWithoutPersisting_When_UserDoesNotExist()
    {
        var userId = Guid.CreateVersion7();
        var repository = Substitute.For<IUsersRepository>();
        repository.GetByIdAsync(Arg.Is<UserId>(id => id.Value == userId), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<User>(Error.NotFound("User was not found.")));
        var handler = new UpdateUserHandler(repository, Substitute.For<ICurrentUser>());
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateUserCommand(userId, "hero@example.com", "hero", true, [], "Active", []),
            cancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "User was not found.");
        await repository.DidNotReceiveWithAnyArgs().UpdateAsync(null!, null, cancellationToken);
    }

    private static User RestoreUser(Guid id, bool isConfirmed = true)
    {
        return User.Restore(
            UserId.Create(id).Value,
            Email.Create("original@example.com").Value,
            UserName.Create("original-hero").Value,
            UserStatus.Active,
            UserConfirmationStatus.From(isConfirmed),
            [],
            []);
    }
}
