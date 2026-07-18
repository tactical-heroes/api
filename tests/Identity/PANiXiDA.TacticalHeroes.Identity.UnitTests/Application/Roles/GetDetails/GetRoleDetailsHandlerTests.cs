using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.GetDetails;

public sealed class GetRoleDetailsHandlerTests
{
    [Fact(DisplayName = "Role details handler should return a role from the read repository")]
    public async Task HandleAsync_Should_ReturnRole_When_RoleExists()
    {
        var roleId = Guid.CreateVersion7();
        var readModel = new RoleDetailsReadModel(roleId, "admin", []);
        var repository = Substitute.For<IRolesReadRepository>();
        repository.GetDetailsByIdAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(readModel);
        var handler = new GetRoleDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetRoleDetailsQuery(roleId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
    }

    [Fact(DisplayName = "Role details handler should return not found for a missing role")]
    public async Task HandleAsync_Should_ReturnNotFound_When_RoleDoesNotExist()
    {
        var repository = Substitute.For<IRolesReadRepository>();
        repository.GetDetailsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((RoleDetailsReadModel?)null);
        var handler = new GetRoleDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetRoleDetailsQuery(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Role was not found.");
    }
}
