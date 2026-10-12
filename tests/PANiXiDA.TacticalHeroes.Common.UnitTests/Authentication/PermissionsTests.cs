using System.Reflection;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Sections;

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
                && !string.IsNullOrWhiteSpace(permission.SectionCode)
                && !string.IsNullOrWhiteSpace(permission.ResourceCode)
                && !string.IsNullOrWhiteSpace(permission.ResourceDisplayName));
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
            .Where(permission => permission.Code.StartsWith($"{resource}.", StringComparison.Ordinal))
            .Select(permission => permission.Code)
            .ToArray();

        permissions.ShouldBe(
            [$"{resource}.create", $"{resource}.read", $"{resource}.update", $"{resource}.delete"],
            ignoreOrder: true);
    }

    [Fact(DisplayName = "All should reference known sections when catalog is read")]
    public void All_Should_ReferenceKnownSections_When_CatalogIsRead()
    {
        var permissions = Permissions.All;

        foreach (var permission in permissions)
        {
            Sections.All.Count(section => section.Code == permission.SectionCode).ShouldBe(1);
            permission.Code.ShouldStartWith($"{permission.SectionCode}.{permission.ResourceCode}.");
        }
    }

    [Fact(DisplayName = "All should use consistent resource names when catalog is grouped")]
    public void All_Should_UseConsistentResourceNames_When_CatalogIsGrouped()
    {
        var resources = Permissions.All
            .GroupBy(permission => (permission.SectionCode, permission.ResourceCode))
            .ToArray();

        resources.Length.ShouldBe(5);
        foreach (var resource in resources)
        {
            resource.Select(permission => permission.ResourceDisplayName)
                .Distinct(StringComparer.Ordinal)
                .Count()
                .ShouldBe(1);
        }
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
