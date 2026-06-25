using Organization.Product.Module.Domain.Users.ValueObjects;

namespace Organization.Product.Module.UnitTests.Domain.Users.ValueObjects;

public sealed class AvatarTests
{
    [Theory(DisplayName = "Create should return null when avatar is not provided")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Return_Null_When_Avatar_Is_Not_Provided(string? avatar)
    {
        var result = Avatar.Create(avatar);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact(DisplayName = "Create should trim avatar when value has surrounding whitespace")]
    public void Create_Should_Trim_Avatar_When_Value_Has_Surrounding_Whitespace()
    {
        var value = "  https://example.com/avatar.png  ";

        var result = Avatar.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Value.ShouldBe("https://example.com/avatar.png");
        result.Value.ToString().ShouldBe("https://example.com/avatar.png");
    }

    [Fact(DisplayName = "Create should return failure when avatar is too long")]
    public void Create_Should_Return_Failure_When_Avatar_Is_Too_Long()
    {
        var value = new string('a', 2049);

        var result = Avatar.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Avatar cannot be longer than 2048 characters.");

        error.ShouldHaveField(nameof(Avatar));
    }

    [Fact(DisplayName = "Equals should compare by value when avatars are equal")]
    public void Equals_Should_Compare_By_Value_When_Avatars_Are_Equal()
    {
        var first = Avatar.Create("https://example.com/avatar.png").Value;
        var second = Avatar.Create("https://example.com/avatar.png").Value;

        first.ShouldBe(second);
    }
}
