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
                && !string.IsNullOrWhiteSpace(permission.SectionCode)
                && !string.IsNullOrWhiteSpace(permission.ResourceCode));
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

    [Fact(DisplayName = "All should reference known sections and resources when catalog is read")]
    public void All_Should_ReferenceKnownSectionsAndResources_When_CatalogIsRead()
    {
        var permissions = Permissions.All;

        foreach (var permission in permissions)
        {
            Sections.All.Count(section => section.Code == permission.SectionCode).ShouldBe(1);
            Permissions.Resources
                .Count(resource =>
                    resource.SectionCode == permission.SectionCode && resource.Code == permission.ResourceCode)
                .ShouldBe(1);
            permission.Code.ShouldStartWith($"{permission.SectionCode}.{permission.ResourceCode}.");
        }
    }

    [Fact(DisplayName = "Resources should contain every declared resource exactly once when catalog is read")]
    public void Resources_Should_ContainEveryDeclaredResourceExactlyOnce_When_CatalogIsRead()
    {
        var declaredResources = typeof(Permissions).GetNestedTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.FieldType == typeof(ResourceDefinition))
            .Select(field => (ResourceDefinition)field.GetValue(null)!)
            .ToArray();

        Permissions.Resources.ShouldNotBeEmpty();
        Permissions.Resources.ShouldBe(declaredResources, ignoreOrder: true);
        Permissions.Resources
            .Select(resource => (resource.SectionCode, resource.Code))
            .Distinct()
            .Count()
            .ShouldBe(Permissions.Resources.Count);
        Permissions.Resources.ShouldAllBe(resource =>
            !string.IsNullOrWhiteSpace(resource.Code) && !string.IsNullOrWhiteSpace(resource.DisplayName));
        foreach (var resource in Permissions.Resources)
        {
            Sections.All.Count(section => section.Code == resource.SectionCode).ShouldBe(1);
            Permissions.All.ShouldContain(permission =>
                permission.SectionCode == resource.SectionCode && permission.ResourceCode == resource.Code);
        }

        var resources = (IList<ResourceDefinition>)Permissions.Resources;
        resources.IsReadOnly.ShouldBeTrue();
        Should.Throw<NotSupportedException>(() => resources.Clear());
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
