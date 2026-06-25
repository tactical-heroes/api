using Organization.Product.Module.Domain.Users.Events;

namespace Organization.Product.Module.UnitTests.Domain.Users.Events;

public sealed class UserEmailChangedTests
{
    [Fact(DisplayName = "Constructor should set event data when arguments are provided")]
    public void Constructor_Should_Set_Event_Data_When_Arguments_Are_Provided()
    {
        var userId = Guid.NewGuid();

        var domainEvent = new UserEmailChanged(
            userId,
            "old@example.com",
            "new@example.com");

        domainEvent.UserId.ShouldBe(userId);
        domainEvent.OldEmail.ShouldBe("old@example.com");
        domainEvent.NewEmail.ShouldBe("new@example.com");
        domainEvent.Id.ShouldNotBe(Guid.Empty);
        domainEvent.OccurredOnUtc.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
    }
}
