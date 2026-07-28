using System.Reflection;
using System.Text.RegularExpressions;

using PANiXiDA.Core.Presentation.Http.Endpoints;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Presentation;

public sealed class EndpointMetadataConventionTests
{
    private static readonly Regex RouteSegmentPattern = new(
        "^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant);

    private static readonly Regex RouteParameterPattern = new(
        "^\\{[a-z][a-z0-9]*(?::[a-z][a-z0-9]*)?\\}$",
        RegexOptions.CultureInvariant);

    private static readonly Regex NamePattern = new(
        "^[A-Z][A-Za-z0-9]*$",
        RegexOptions.CultureInvariant);

    private static readonly Regex SummaryPattern = new(
        "^[A-Z][A-Za-z0-9]*(?: [A-Za-z0-9]+)*$",
        RegexOptions.CultureInvariant);

    [Fact(DisplayName = "Endpoint metadata should follow HTTP naming conventions when endpoint is declared")]
    public void EndpointMetadata_Should_FollowNamingConventions_When_EndpointIsDeclared()
    {
        var metadataTypes = ArchitectureDefinition.ProductionAssemblies
            .Where(assembly => assembly.GetName().Name?.EndsWith(
                ".Presentation",
                StringComparison.Ordinal) == true)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Select(type => new
            {
                Type = type,
                IsEndpoint = typeof(IEndpoint).IsAssignableFrom(type),
                IsEndpointGroup = typeof(IEndpointGroup).IsAssignableFrom(type)
            })
            .Where(metadata => metadata.IsEndpoint || metadata.IsEndpointGroup)
            .OrderBy(metadata => metadata.Type.FullName, StringComparer.Ordinal)
            .ToArray();

        var violations = new List<string>();

        foreach (var metadata in metadataTypes)
        {
            var instance = Activator.CreateInstance(metadata.Type, nonPublic: true)
                ?? throw new InvalidOperationException($"Could not create '{metadata.Type.FullName}'.");
            var route = GetMetadata(instance, "Route");
            var name = GetMetadata(instance, "Name");

            if (!IsValidRoute(route))
            {
                violations.Add(
                    $"{metadata.Type.FullName}.Route must use lowercase English kebab-case: '{route}'.");
            }

            if (!NamePattern.IsMatch(name))
            {
                violations.Add(
                    $"{metadata.Type.FullName}.Name must be one English PascalCase identifier: '{name}'.");
            }

            if (!metadata.IsEndpoint)
            {
                continue;
            }

            var summary = GetMetadata(instance, "Summary");
            if (!SummaryPattern.IsMatch(summary))
            {
                violations.Add(
                    $"{metadata.Type.FullName}.Summary must be English sentence case with single spaces: '{summary}'.");
            }
        }

        Assert.True(
            violations.Count == 0,
            string.Join(Environment.NewLine, violations));
    }

    private static string GetMetadata(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);

        return property?.GetValue(instance) as string ?? string.Empty;
    }

    private static bool IsValidRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return false;
        }

        return route
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .All(segment =>
                RouteSegmentPattern.IsMatch(segment) ||
                RouteParameterPattern.IsMatch(segment));
    }
}
