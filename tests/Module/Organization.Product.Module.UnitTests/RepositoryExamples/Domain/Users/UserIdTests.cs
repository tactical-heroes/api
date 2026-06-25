using Organization.Product.Module.Domain.Users;

namespace Organization.Product.Module.UnitTests.Domain.Users;

public sealed class UserIdTests
{
    [Fact(DisplayName = "New should create non-empty id when called")]
    public void New_Should_Create_Non_Empty_Id_When_Called()
    {
        var id = UserId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Create should return id when value is not empty")]
    public void Create_Should_Return_Id_When_Value_Is_Not_Empty()
    {
        var value = Guid.NewGuid();

        var result = UserId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Create should return failure when value is empty")]
    public void Create_Should_Return_Failure_When_Value_Is_Empty()
    {
        var value = Guid.Empty;

        var result = UserId.Create(value);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "User id cannot be empty.");
    }
}
