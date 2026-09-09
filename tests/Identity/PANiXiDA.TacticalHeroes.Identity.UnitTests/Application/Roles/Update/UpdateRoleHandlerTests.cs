using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Update;

public sealed class UpdateRoleHandlerTests
{
    [Fact(DisplayName = "Update role handler should delegate update to the write repository when repository succeeds")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var roleId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IRolesWriteRepository>();
        repository.UpdateAsync(Arg.Is<Role>(role => role.Id.Value == roleId && role.Name.Value == "admin" && role.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new UpdateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateRoleCommand(roleId, "admin", claims),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(Arg.Is<Role>(role => role.Id.Value == roleId && role.Name.Value == "admin" && role.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")), cancellationToken);
    }
}
