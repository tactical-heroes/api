using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.Enumerations;

public sealed class RoleNamesTests
{
    [Fact(DisplayName = "Default roles should retain their identifier order when called")]
    public void GetAll_Should_ReturnRolesInIdOrder_When_Called()
    {
        var roles = RoleNames.GetAll();

        roles.ShouldBe([RoleNames.Developer, RoleNames.Moderator, RoleNames.Player]);
    }

    [Theory(DisplayName = "Default role should resolve its name when id is known")]
    [InlineData(1, "developer")]
    [InlineData(2, "moderator")]
    [InlineData(3, "player")]
    public void FromId_Should_ReturnRole_When_IdIsKnown(int id, string name)
    {
        var role = RoleNames.FromId(id);

        role.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Default role should reject an unknown identifier when id is unknown")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Action action = () => RoleNames.FromId(0);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "Default role should resolve a trimmed name when name is known")]
    [InlineData("developer", 1)]
    [InlineData(" moderator ", 2)]
    [InlineData("player", 3)]
    public void FromName_Should_ReturnRole_When_NameIsKnown(string name, int id)
    {
        var role = RoleNames.FromName(name);

        role.Id.ShouldBe(id);
    }

    [Theory(DisplayName = "Default role should reject an invalid name when name is invalid")]
    [InlineData("admin")]
    [InlineData("Developer")]
    [InlineData("   ")]
    public void FromName_Should_Throw_When_NameIsInvalid(string name)
    {
        Action action = () => RoleNames.FromName(name);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "Default role lookup should report a match when id is provided")]
    [InlineData(1, "developer")]
    [InlineData(2, "moderator")]
    [InlineData(3, "player")]
    [InlineData(0, null)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, string? expectedName)
    {
        var found = RoleNames.TryFromId(id, out var role);

        found.ShouldBe(expectedName is not null);
        (role?.Name).ShouldBe(expectedName);
    }

    [Theory(DisplayName = "Default role lookup should report a match when name is provided")]
    [InlineData("developer", 1)]
    [InlineData(" moderator ", 2)]
    [InlineData("player", 3)]
    [InlineData("admin", null)]
    [InlineData("Developer", null)]
    [InlineData("   ", null)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, int? expectedId)
    {
        var found = RoleNames.TryFromName(name, out var role);

        found.ShouldBe(expectedId.HasValue);
        (role?.Id).ShouldBe(expectedId);
    }
}
