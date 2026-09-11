using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Delete;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Delete;

public sealed class DeleteRoleHandlerTests
{
    [Fact(DisplayName = "Delete role handler should delegate deletion to the write repository when repository succeeds")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var roleId = Guid.CreateVersion7();
        var repository = Substitute.For<IRolesRepository>();
        repository.DeleteAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new DeleteRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new DeleteRoleCommand(roleId), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).DeleteAsync(roleId, cancellationToken);
    }
}
