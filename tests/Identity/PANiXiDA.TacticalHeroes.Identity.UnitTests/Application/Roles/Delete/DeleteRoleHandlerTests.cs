using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Delete;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Delete;

public sealed class DeleteRoleHandlerTests
{
    [Fact(
        DisplayName = "Delete role handler should delegate deletion to the write repository when repository succeeds")]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds()
    {
        var roleId = Guid.CreateVersion7();
        var repository = Substitute.For<IRolesRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.AccessToken.Returns("current-access-token");
        repository.DeleteAsync(roleId, "current-access-token", Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new DeleteRoleHandler(repository, currentUser);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new DeleteRoleCommand(roleId), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).DeleteAsync(roleId, "current-access-token", cancellationToken);
    }
}
