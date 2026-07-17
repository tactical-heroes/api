using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.UseCases;

public sealed class UseCaseDelegationTests
{
    [Fact(DisplayName = "User details use case should query its port once")]
    public async Task HandleAsync_Should_QueryUserPortOnce_When_GettingDetails()
    {
        var id = Guid.CreateVersion7();
        var readModel = new UserDetailsReadModel(
            Id: id,
            Email: "hero@example.com",
            UserName: "hero",
            IsConfirmed: true,
            Status: "Active",
            StatusDisplayName: "Active",
            Claims: []);
        var repository = Substitute.For<IUsersReadRepository>();
        repository.GetDetailsByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Result.Success(value: readModel));
        var handler = new GetUserDetailsHandler(usersRepository: repository);

        var result = await handler.HandleAsync(
            new GetUserDetailsQuery(Id: id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await repository.Received(1).GetDetailsByIdAsync(
            id,
            TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Login use case should call its credentials port once")]
    public async Task HandleAsync_Should_CallCredentialsPortOnce_When_LoggingIn()
    {
        const string email = "hero@example.com";
        const string password = "Password-1";
        var readModel = new AuthenticatedUserReadModel(
            Id: Guid.CreateVersion7(),
            Email: email,
            UserName: "hero",
            Claims: []);
        var service = Substitute.For<IUserCredentialsService>();
        service.LoginAsync(email, password, Arg.Any<CancellationToken>())
            .Returns(Result.Success(value: readModel));
        var handler = new LoginHandler(userCredentialsService: service);

        var result = await handler.HandleAsync(
            new LoginCommand(Email: email, Password: password),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await service.Received(1).LoginAsync(
            email,
            password,
            TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Role details use case should query its port once")]
    public async Task HandleAsync_Should_QueryRolePortOnce_When_GettingDetails()
    {
        var id = Guid.CreateVersion7();
        var readModel = new RoleDetailsReadModel(Id: id, Name: "Administrator", Claims: []);
        var repository = Substitute.For<IRolesReadRepository>();
        repository.GetDetailsByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Result.Success(value: readModel));
        var handler = new GetRoleDetailsHandler(rolesRepository: repository);

        var result = await handler.HandleAsync(
            new GetRoleDetailsQuery(Id: id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await repository.Received(1).GetDetailsByIdAsync(
            id,
            TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "OAuth user info use case should query its port once")]
    public async Task HandleAsync_Should_QueryOAuthPortOnce_When_GettingUserInfo()
    {
        var id = Guid.CreateVersion7();
        var readModel = new UserInfoReadModel(
            UserId: id,
            Name: "Hero",
            Email: "hero@example.com",
            EmailVerified: true,
            Roles: ["Administrator"]);
        var repository = Substitute.For<IOAuthUsersRepository>();
        repository.GetUserInfoByUserIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Result.Success(value: readModel));
        var handler = new GetUserInfoHandler(usersRepository: repository);

        var result = await handler.HandleAsync(
            new GetUserInfoQuery(UserId: id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await repository.Received(1).GetUserInfoByUserIdAsync(
            id,
            TestContext.Current.CancellationToken);
    }
}
