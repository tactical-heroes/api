using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.Enumerations;

public sealed class UserStatusTests
{
    [Fact(DisplayName = "User statuses should retain their identifier order when listed")]
    public void GetAll_Should_ReturnStatusesInIdOrder_When_Called()
    {
        UserStatus.GetAll().ShouldBe([UserStatus.Active, UserStatus.Blocked]);
    }

    [Theory(DisplayName = "User status should resolve a known identifier when identifier is known")]
    [InlineData(1, "Active")]
    [InlineData(2, "Blocked")]
    public void FromId_Should_ReturnStatus_When_IdIsKnown(int id, string name)
    {
        UserStatus.FromId(id).Name.ShouldBe(name);
    }

    [Fact(DisplayName = "User status should reject an unknown identifier when resolving by identifier")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Should.Throw<InvalidOperationException>(() => UserStatus.FromId(0));
    }

    [Theory(DisplayName = "User status should resolve a trimmed name when name is known")]
    [InlineData("Active", 1)]
    [InlineData("  Blocked  ", 2)]
    public void FromName_Should_ReturnStatus_When_NameIsKnown(string name, int id)
    {
        UserStatus.FromName(name).Id.ShouldBe(id);
    }

    [Theory(DisplayName = "User status should reject an invalid name when resolving by name")]
    [InlineData("Deleted")]
    [InlineData("active")]
    [InlineData("   ")]
    public void FromName_Should_Throw_When_NameIsInvalid(string name)
    {
        Should.Throw<InvalidOperationException>(() => UserStatus.FromName(name));
    }

    [Theory(DisplayName = "User status lookup should report whether an identifier is known")]
    [InlineData(1, "Active")]
    [InlineData(2, "Blocked")]
    [InlineData(0, null)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, string? expectedName)
    {
        var found = UserStatus.TryFromId(id, out var status);

        found.ShouldBe(expectedName is not null);
        (status?.Name).ShouldBe(expectedName);
    }

    [Theory(DisplayName = "User status lookup should report whether a trimmed name is known")]
    [InlineData("Active", 1)]
    [InlineData("  Blocked  ", 2)]
    [InlineData("Deleted", null)]
    [InlineData("active", null)]
    [InlineData("   ", null)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, int? expectedId)
    {
        var found = UserStatus.TryFromName(name, out var status);

        found.ShouldBe(expectedId.HasValue);
        (status?.Id).ShouldBe(expectedId);
    }

    [Theory(DisplayName = "User status should resolve a known name when name is known")]
    [InlineData("Active", false)]
    [InlineData("Blocked", true)]
    public void Create_Should_ReturnStatus_When_NameIsKnown(
        string name,
        bool isBlocked)
    {
        var result = UserStatus.Create(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsBlocked.ShouldBe(isBlocked);
    }

    [Fact(DisplayName = "User status should trim a known name when name is known")]
    public void Create_Should_TrimValue_When_NameIsKnown()
    {
        var result = UserStatus.Create("  Active  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(UserStatus.Active);
    }

    [Fact(DisplayName = "User status should reject an empty value when value is empty")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsEmpty()
    {
        var result = UserStatus.Create("   ");

        result.ShouldHaveSingleError(ErrorType.Validation, "User status is required.")
            .ShouldHaveField(nameof(UserStatus));
    }

    [Fact(DisplayName = "User status should reject an unknown value when value is unknown")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsUnknown()
    {
        var result = UserStatus.Create("Deleted");

        result.ShouldHaveSingleError(ErrorType.Validation, "User status 'Deleted' is invalid.")
            .ShouldHaveField(nameof(UserStatus));
    }
}
