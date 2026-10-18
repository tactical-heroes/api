using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.Enumerations;

public sealed class RoleNameTests
{
    [Fact(DisplayName = "Default roles should retain their identifier order when called")]
    public void GetAll_Should_ReturnRolesInIdOrder_When_Called()
    {
        var roles = RoleName.GetAll();

        roles.ShouldBe([RoleName.Developer, RoleName.Moderator, RoleName.Player]);
    }

    [Theory(DisplayName = "Default role should resolve its name when id is known")]
    [InlineData(1, "Developer", "Разработчик")]
    [InlineData(2, "Moderator", "Модератор")]
    [InlineData(3, "Player", "Игрок")]
    public void FromId_Should_ReturnRole_When_IdIsKnown(int id, string name, string displayName)
    {
        var role = RoleName.FromId(id);

        role.Name.ShouldBe(name);
        role.DisplayName.ShouldBe(displayName);
    }

    [Fact(DisplayName = "Default role should reject an unknown identifier when id is unknown")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Action action = () => RoleName.FromId(0);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "Default role should resolve a trimmed name when name is known")]
    [InlineData("Developer", 1)]
    [InlineData(" Moderator ", 2)]
    [InlineData("Player", 3)]
    public void FromName_Should_ReturnRole_When_NameIsKnown(string name, int id)
    {
        var role = RoleName.FromName(name);

        role.Id.ShouldBe(id);
    }

    [Theory(DisplayName = "Default role should reject an invalid name when name is invalid")]
    [InlineData("admin")]
    [InlineData("developer")]
    [InlineData("   ")]
    public void FromName_Should_Throw_When_NameIsInvalid(string name)
    {
        Action action = () => RoleName.FromName(name);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "Default role lookup should report a match when id is provided")]
    [InlineData(1, "Developer")]
    [InlineData(2, "Moderator")]
    [InlineData(3, "Player")]
    [InlineData(0, null)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, string? expectedName)
    {
        var found = RoleName.TryFromId(id, out var role);

        found.ShouldBe(expectedName is not null);
        (role?.Name).ShouldBe(expectedName);
    }

    [Theory(DisplayName = "Default role lookup should report a match when name is provided")]
    [InlineData("Developer", 1)]
    [InlineData(" Moderator ", 2)]
    [InlineData("Player", 3)]
    [InlineData("admin", null)]
    [InlineData("developer", null)]
    [InlineData("   ", null)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, int? expectedId)
    {
        var found = RoleName.TryFromName(name, out var role);

        found.ShouldBe(expectedId.HasValue);
        (role?.Id).ShouldBe(expectedId);
    }
}
