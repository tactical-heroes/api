using PANiXiDA.Core.Presentation.Http.Endpoints;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Presentation;

public sealed class EndpointVerticalSliceConventionTests
{
    private const string EndpointSuffix = "Endpoint";
    private const string EndpointGroupSuffix = "Endpoints";
    private const string FeaturesNamespaceSegment = "Features";

    [Fact(DisplayName = "Aggregate roots should have endpoint groups when declared")]
    public void AggregateRoots_Should_HaveEndpointGroups_When_Declared()
    {
        var endpointGroups = PresentationArchitectureConvention.GetEndpointGroups();
        var aggregates = ArchitectureDefinition.s_modules
            .SelectMany(module => InfrastructurePersistenceConvention.GetAggregateRootTypes(module)
                .Select(aggregate => new { Module = module, Aggregate = aggregate }))
            .ToArray();
        var violations = aggregates
            .Select(target =>
            {
                var feature = EnglishNamingConvention.Pluralize(target.Aggregate.Name);
                var expectedName = $"{target.Module.PresentationAssemblyName}.Features.{feature}.{feature}Endpoints";

                return endpointGroups.Count(group => group.FullName == expectedName) == 1
                    ? null
                    : $"{target.Aggregate.FullName} must have endpoint group '{expectedName}'.";
            })
            .OfType<string>()
            .ToArray();

        Assert.NotEmpty(aggregates);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Endpoint groups should reside in feature roots and match feature names when declared")]
    public void EndpointGroups_Should_ResideInFeatureRootsAndMatchFeatureNames_When_Declared()
    {
        var endpointGroups =
            PresentationArchitectureConvention.GetEndpointGroups();
        var violations = endpointGroups
            .SelectMany(GetEndpointGroupViolations)
            .ToArray();

        Assert.NotEmpty(endpointGroups);
        Assert.True(
            violations.Length == 0,
            $"Endpoint group convention violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Endpoints should reside in feature slices under their generic groups when declared")]
    public void Endpoints_Should_ResideInFeatureSlicesUnderTheirGenericGroups_When_Declared()
    {
        var endpoints = PresentationArchitectureConvention.GetEndpoints();
        var violations = endpoints
            .SelectMany(GetEndpointLocationViolations)
            .ToArray();

        Assert.NotEmpty(endpoints);
        Assert.True(
            violations.Length == 0,
            $"Endpoint vertical slice violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Endpoints should end with Endpoint when declared")]
    public void Endpoints_Should_EndWithEndpoint_When_Declared()
    {
        var endpoints = PresentationArchitectureConvention.GetEndpoints();
        var violations = endpoints
            .Where(endpoint => !endpoint.Name.EndsWith(
                EndpointSuffix,
                StringComparison.Ordinal))
            .Select(endpoint =>
                $"{endpoint.FullName} must end with '{EndpointSuffix}'.")
            .ToArray();

        Assert.NotEmpty(endpoints);
        Assert.True(
            violations.Length == 0,
            $"Endpoint naming violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<string> GetEndpointGroupViolations(
        Type endpointGroup)
    {
        var relativeNamespaceSegments =
            PresentationArchitectureConvention
                .GetRelativeNamespace(endpointGroup)
                .Split(
                    '.',
                    StringSplitOptions.RemoveEmptyEntries);

        if (relativeNamespaceSegments.Length != 2 ||
            !string.Equals(
                relativeNamespaceSegments[0],
                FeaturesNamespaceSegment,
                StringComparison.Ordinal))
        {
            return
            [
                $"{endpointGroup.FullName} must reside directly under " +
                $"'{FeaturesNamespaceSegment}/<AggregatePlural>'."
            ];
        }

        var featureName = relativeNamespaceSegments[1];
        var expectedTypeName = featureName + EndpointGroupSuffix;
        var instance = Activator.CreateInstance(
            endpointGroup,
            nonPublic: true)
            ?? throw new InvalidOperationException(
                $"Could not create '{endpointGroup.FullName}'.");
        var groupName = endpointGroup
            .GetProperty(nameof(IEndpointGroup.Name))
            ?.GetValue(instance) as string;
        var violations = new List<string>();

        if (!string.Equals(
                endpointGroup.Name,
                expectedTypeName,
                StringComparison.Ordinal))
        {
            violations.Add(
                $"{endpointGroup.FullName} must be named " +
                $"'{expectedTypeName}'.");
        }

        if (!string.Equals(
                groupName,
                featureName,
                StringComparison.Ordinal))
        {
            violations.Add(
                $"{endpointGroup.FullName}.Name must be " +
                $"'{featureName}'.");
        }

        violations.AddRange(
            PresentationArchitectureConvention.GetLocationViolations(
                endpointGroup,
                FeaturesNamespaceSegment,
                featureName));

        return violations;
    }

    private static IEnumerable<string> GetEndpointLocationViolations(
        Type endpoint)
    {
        var endpointGroup =
            PresentationArchitectureConvention.GetEndpointGroup(endpoint);
        var endpointNamespace = endpoint.Namespace
            ?? throw new InvalidOperationException(
                $"Endpoint '{endpoint.FullName}' does not have a namespace.");
        var groupNamespace = endpointGroup.Namespace
            ?? throw new InvalidOperationException(
                $"Endpoint group '{endpointGroup.FullName}' does not have a " +
                $"namespace.");
        var expectedNamespacePrefix = groupNamespace + ".";

        if (!endpointNamespace.StartsWith(
                expectedNamespacePrefix,
                StringComparison.Ordinal))
        {
            return
            [
                $"{endpoint.FullName} must reside in a feature folder " +
                $"below its generic endpoint group " +
                $"'{endpointGroup.FullName}'."
            ];
        }

        var relativeFeatureSegments =
            endpointNamespace[expectedNamespacePrefix.Length..]
                .Split(
                    '.',
                    StringSplitOptions.RemoveEmptyEntries);

        if (relativeFeatureSegments.Length == 0)
        {
            return
            [
                $"{endpoint.FullName} must reside in a concrete feature " +
                $"folder below '{groupNamespace}'."
            ];
        }

        var groupRelativeNamespace =
            PresentationArchitectureConvention
                .GetRelativeNamespace(endpointGroup)
                .Split(
                    '.',
                    StringSplitOptions.RemoveEmptyEntries);

        return PresentationArchitectureConvention.GetLocationViolations(
            endpoint,
            [
                .. groupRelativeNamespace,
                .. relativeFeatureSegments
            ]);
    }
}
