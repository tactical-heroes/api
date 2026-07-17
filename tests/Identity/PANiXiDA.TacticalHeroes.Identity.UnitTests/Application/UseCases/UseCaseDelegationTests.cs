using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.UseCases;

public sealed class UseCaseDelegationTests
{
    [Fact(DisplayName = "Account details use case should query its port once")]
    public async Task HandleAsync_Should_QueryAccountPortOnce_When_GettingDetails()
    {
        var id = Guid.CreateVersion7();
        var readModel = new AccountDetailsReadModel(
            id,
            "hero@example.com",
            "hero",
            true,
            "Active",
            "Active",
            []);
        var repository = Substitute.For<IAccountsReadRepository>();
        repository.GetDetailsByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Result.Success(readModel));
        var handler = new GetAccountDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetAccountDetailsQuery(id),
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
        var readModel = new AuthenticatedAccountReadModel(
            Guid.CreateVersion7(),
            email,
            "hero",
            []);
        var service = Substitute.For<IAccountCredentialsService>();
        service.LoginAsync(email, password, Arg.Any<CancellationToken>())
            .Returns(Result.Success(readModel));
        var handler = new LoginHandler(service);

        var result = await handler.HandleAsync(
            new LoginCommand(email, password),
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
        var readModel = new RoleDetailsReadModel(id, "Administrator", []);
        var repository = Substitute.For<IRolesReadRepository>();
        repository.GetDetailsByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Result.Success(readModel));
        var handler = new GetRoleDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetRoleDetailsQuery(id),
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
            id,
            "Hero",
            "hero@example.com",
            true,
            ["Administrator"]);
        var repository = Substitute.For<IOAuthUsersRepository>();
        repository.GetUserInfoByAccountIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Result.Success(readModel));
        var handler = new GetUserInfoHandler(repository);

        var result = await handler.HandleAsync(
            new GetUserInfoQuery(id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await repository.Received(1).GetUserInfoByAccountIdAsync(
            id,
            TestContext.Current.CancellationToken);
    }
}
