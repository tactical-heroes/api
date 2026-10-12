using System.Reflection;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Sections;

namespace PANiXiDA.TacticalHeroes.Common.UnitTests.Authentication;

public sealed class SectionsTests
{
    [Fact(DisplayName = "All should contain every declared section exactly once when catalog is read")]
    public void All_Should_ContainEveryDeclaredSectionExactlyOnce_When_CatalogIsRead()
    {
        var declaredSections = typeof(Sections).GetNestedTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.FieldType == typeof(SectionDefinition))
            .Select(field => (SectionDefinition)field.GetValue(null)!)
            .ToArray();

        Sections.All.ShouldNotBeEmpty();
        Sections.All.ShouldBe(declaredSections, ignoreOrder: true);
        Sections.All.Select(section => section.Code).ShouldBe(["identity", "compendium"]);
        Sections.All.ShouldAllBe(section => !string.IsNullOrWhiteSpace(section.DisplayName));
        foreach (var section in Sections.All)
        {
            Permissions.All.ShouldContain(permission => permission.SectionCode == section.Code);
        }

        var sections = (IList<SectionDefinition>)Sections.All;
        sections.IsReadOnly.ShouldBeTrue();
        Should.Throw<NotSupportedException>(() => sections.Clear());
    }
}
