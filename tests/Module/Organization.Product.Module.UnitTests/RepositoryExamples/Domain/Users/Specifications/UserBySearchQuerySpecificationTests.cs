using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Specifications;

namespace Organization.Product.Module.UnitTests.Domain.Users.Specifications;

public sealed class UserBySearchQuerySpecificationTests
{
    [Theory(DisplayName = "IsSatisfiedBy should match name or email when search query is provided")]
    [InlineData("John", true)]
    [InlineData("john@example.com", true)]
    [InlineData("12345678901", false)]
    public void IsSatisfiedBy_Should_Match_Name_Or_Email_When_Search_Query_Is_Provided(
        string searchQuery,
        bool expected)
    {
        var user = User.Create(
            role: "User",
            name: "John Doe",
            email: "john@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var specification = new UserBySearchQuerySpecification(searchQuery);

        specification.IsSatisfiedBy(user).ShouldBe(expected);
        specification.ToExpression().Compile()(user).ShouldBe(expected);
    }
}
