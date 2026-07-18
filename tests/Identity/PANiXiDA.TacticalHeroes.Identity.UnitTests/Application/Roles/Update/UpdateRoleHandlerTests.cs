using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Update;

public sealed class UpdateRoleHandlerTests
{
    [Fact(DisplayName = "Update role handler should delegate update to the write repository")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var roleId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IRolesWriteRepository>();
        repository.UpdateAsync(roleId, "admin", claims, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new UpdateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateRoleCommand(roleId, "admin", claims),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(roleId, "admin", claims, cancellationToken);
    }
}
