using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Create;

public sealed class CreateRoleHandlerTests
{
    [Fact(DisplayName = "Create role handler should delegate creation to the write repository")]
    public async Task HandleAsync_Should_ReturnRoleId_When_RepositorySucceeds()
    {
        var roleId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IRolesWriteRepository>();
        repository.AddAsync("admin", claims, Arg.Any<CancellationToken>())
            .Returns(Result.Success(roleId));
        var handler = new CreateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateRoleCommand("admin", claims),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(roleId);
        await repository.Received(1).AddAsync("admin", claims, cancellationToken);
    }
}
