using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Specifications;

namespace Organization.Product.Module.UnitTests.Domain.Users.Specifications;

public sealed class UserByRoleSpecificationTests
{
    [Theory(DisplayName = "IsSatisfiedBy should match role when role differs by case")]
    [InlineData("admin", true)]
    [InlineData("User", false)]
    public void IsSatisfiedBy_Should_Match_Role_When_Role_Differs_By_Case(
        string role,
        bool expected)
    {
        var user = User.Create(
            role: "Admin",
            name: "John Doe",
            email: "john.doe@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var specification = new UserByRoleSpecification(role);

        specification.IsSatisfiedBy(user).ShouldBe(expected);
        specification.ToExpression().Compile()(user).ShouldBe(expected);
    }
}
