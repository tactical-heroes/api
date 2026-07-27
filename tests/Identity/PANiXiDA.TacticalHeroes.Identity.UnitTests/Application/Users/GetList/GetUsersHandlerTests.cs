using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetList;

public sealed class GetUsersHandlerTests
{
    [Fact(DisplayName = "User list handler should return a filtered page from the read repository")]
    public async Task HandleAsync_Should_ReturnPage_When_RepositorySucceeds()
    {
        var pagination = new PaginationParameters(1, 20);
        var page = PaginationResult<UserListItemReadModel>.Create(
            [new UserListItemReadModel(
                Guid.CreateVersion7(),
                "hero@example.com",
                "hero",
                true,
                "Active",
                "Active")],
            1,
            20,
            1);
        var usersReadRepository = Substitute.For<IUsersReadRepository>();
        usersReadRepository.GetPagedAsync(
                "hero@example.com",
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(page);
        var handler = new GetUsersHandler(usersReadRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new GetUsersQuery("hero@example.com", pagination),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(page);
        await usersReadRepository.Received(1).GetPagedAsync(
            "hero@example.com",
            pagination,
            cancellationToken);
    }
}
