using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.GetList;

public sealed class GetRolesHandlerTests
{
    [Fact(DisplayName = "Role list handler should return a page from the read repository")]
    public async Task HandleAsync_Should_ReturnPage_When_RepositorySucceeds()
    {
        var pagination = new PaginationParameters(1, 20);
        var page = PaginationResult<RoleListItemReadModel>.Create(
            [new RoleListItemReadModel(Guid.CreateVersion7(), "admin")],
            1,
            20,
            1);
        var repository = Substitute.For<IRolesReadRepository>();
        repository.GetPagedAsync(pagination, Arg.Any<CancellationToken>())
            .Returns(page);
        var handler = new GetRolesHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new GetRolesQuery(pagination),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(page);
        await repository.Received(1).GetPagedAsync(pagination, cancellationToken);
    }
}
