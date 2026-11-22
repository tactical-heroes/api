using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.GetSelectOptions;

public sealed class GetRoleSelectOptionsHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return repository options when repository succeeds")]
    public async Task HandleAsync_Should_ReturnRepositoryOptions_When_RepositorySucceeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = Substitute.For<IRolesReadRepository>();
        List<RoleSelectOptionReadModel> options = [new(Guid.NewGuid(), "Player", true)];
        repository.GetSelectOptionsAsync(cancellationToken).Returns(options);
        var handler = new GetRoleSelectOptionsHandler(repository);

        var result = await handler.HandleAsync(new GetRoleSelectOptionsQuery(), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(options);
        await repository.Received(1).GetSelectOptionsAsync(cancellationToken);
    }
}
