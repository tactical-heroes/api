using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetStatuses;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetStatuses;

public sealed class GetUserStatusesHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return all user statuses")]
    public async Task HandleAsync_Should_ReturnAllUserStatuses()
    {
        var handler = new GetUserStatusesHandler();

        var result = await handler.HandleAsync(
            new GetUserStatusesQuery(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(UserStatus.GetAll().Count);

        foreach (var status in UserStatus.GetAll())
        {
            var readModel = result.Value.Single(item => item.Id == status.Id);
            readModel.Name.ShouldBe(status.Name);
            readModel.DisplayName.ShouldBe(status.DisplayName);
        }
    }
}
