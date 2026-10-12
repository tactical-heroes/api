using System.Reflection;

using PANiXiDA.TacticalHeroes.Common.Authentication;

namespace PANiXiDA.TacticalHeroes.Common.UnitTests.Authentication;

public sealed class PermissionsTests
{
    [Fact(DisplayName = "All should contain every declared permission exactly once when catalog is read")]
    public void All_Should_ContainEveryDeclaredPermissionExactlyOnce_When_CatalogIsRead()
    {
        var declaredPermissions = typeof(Permissions).GetNestedTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.FieldType == typeof(PermissionDefinition))
            .Select(field => (PermissionDefinition)field.GetValue(null)!)
            .ToArray();

        Permissions.All.ShouldNotBeEmpty();
        Permissions.All.ShouldBe(declaredPermissions, ignoreOrder: true);
        Permissions.All
            .Select(permission => permission.Code)
            .Distinct(StringComparer.Ordinal)
            .Count()
            .ShouldBe(Permissions.All.Count);
        Permissions.All.ShouldAllBe(permission =>
            !string.IsNullOrWhiteSpace(permission.Code)
                && !string.IsNullOrWhiteSpace(permission.DisplayName)
                && !string.IsNullOrWhiteSpace(permission.Group));
    }

    [Theory(DisplayName = "All should preserve permission codes when catalog is read")]
    [InlineData("identity.users")]
    [InlineData("identity.roles")]
    [InlineData("compendium.factions")]
    [InlineData("compendium.heroes")]
    [InlineData("compendium.units")]
    public void All_Should_PreservePermissionCodes_When_CatalogIsRead(string resource)
    {
        var permissions = Permissions.All
            .Where(permission => permission.Code.StartsWith(resource + ".", StringComparison.Ordinal))
            .Select(permission => permission.Code)
            .ToArray();

        permissions.ShouldBe(
            [resource + ".create", resource + ".read", resource + ".update", resource + ".delete"],
            ignoreOrder: true);
    }

    [Fact(DisplayName = "All should reject collection changes when accessed as mutable list")]
    public void All_Should_RejectCollectionChanges_When_AccessedAsMutableList()
    {
        var permissions = (IList<PermissionDefinition>)Permissions.All;

        permissions.IsReadOnly.ShouldBeTrue();
        Should.Throw<NotSupportedException>(() => permissions.Clear());
        Should.Throw<NotSupportedException>(() => permissions[0] = Permissions.IdentityRoles.Update);
    }
}
