using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetStatuses;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Accounts.Management.GetStatuses;

public sealed class GetAccountStatusesHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return all account statuses")]
    public async Task HandleAsync_Should_ReturnAllAccountStatuses()
    {
        var handler = new GetAccountStatusesHandler();

        var result = await handler.HandleAsync(
            new GetAccountStatusesQuery(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(AccountStatus.GetAll().Count);

        foreach (var status in AccountStatus.GetAll())
        {
            var readModel = result.Value.Single(item => item.Id == status.Id);
            readModel.Name.ShouldBe(status.Name);
            readModel.DisplayName.ShouldBe(status.DisplayName);
        }
    }
}
