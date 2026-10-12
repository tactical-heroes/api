namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public sealed record PermissionDefinition(
    string Code,
    string DisplayName,
    string SectionCode,
    string ResourceCode,
    string ResourceDisplayName)
{
    internal static (
        PermissionDefinition Create,
        PermissionDefinition Read,
        PermissionDefinition Update,
        PermissionDefinition Delete) CreateCrud(
            string sectionCode,
            string resourceCode,
            string resourceDisplayName,
            string resourceGenitiveName)
    {
        PermissionDefinition Define(string action, string displayName) => new(
            Code: $"{sectionCode}.{resourceCode}.{action}",
            DisplayName: displayName,
            SectionCode: sectionCode,
            ResourceCode: resourceCode,
            ResourceDisplayName: resourceDisplayName);

        return (
            Create: Define("create", $"Создание {resourceGenitiveName}"),
            Read: Define("read", $"Просмотр {resourceGenitiveName}"),
            Update: Define("update", $"Изменение {resourceGenitiveName}"),
            Delete: Define("delete", $"Удаление {resourceGenitiveName}"));
    }
}
