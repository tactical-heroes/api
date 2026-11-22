using Microsoft.AspNetCore.SignalR;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Presentation;

public sealed class StreamingLayoutConventionTests
{
    [Fact(DisplayName = "SignalR hubs and their types should follow the streaming layout when declared")]
    public void SignalRHubs_Should_FollowStreamingLayout_When_Declared()
    {
        var hubs = ArchitectureDefinition.s_productionAssemblies
            .Where(assembly =>
                assembly.GetName().Name?.EndsWith(".Presentation", StringComparison.Ordinal) == true)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && typeof(Hub).IsAssignableFrom(type))
            .ToArray();
        var violations = hubs.SelectMany(GetStreamingLayoutViolations).ToArray();

        Assert.NotEmpty(hubs);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<string> GetStreamingLayoutViolations(Type hub)
    {
        var assemblyNamespacePrefix = hub.Assembly.GetName().Name + ".";
        var hubNamespace = hub.Namespace;
        if (hubNamespace is null
            || !hubNamespace.StartsWith(assemblyNamespacePrefix, StringComparison.Ordinal)
            || hubNamespace[assemblyNamespacePrefix.Length..].Split('.') is not
                ["Features", var feature, "Streaming", var stream])
        {
            return [$"{hub.FullName} must reside in Features/<Feature>/Streaming/<Stream>."];
        }

        string[] streamSegments = ["Features", feature, "Streaming", stream];
        var violations = PresentationArchitectureConvention.GetLocationViolations(hub, streamSegments).ToList();
        var clientContract = GetClientContract(hub);
        if (clientContract is not null)
        {
            if (clientContract.IsInterface && clientContract.Assembly == hub.Assembly)
            {
                violations.AddRange(PresentationArchitectureConvention.GetLocationViolations(
                    clientContract, streamSegments));
            }
            else
            {
                violations.Add($"{clientContract.FullName} must be an interface declared alongside {hub.FullName}.");
            }
        }

        var streamNamespacePrefix = hubNamespace + ".";
        var streamTypes = hub.Assembly.GetTypes()
            .Where(type =>
                !type.IsNested
                    && type != hub
                    && type != clientContract
                    && (type.Namespace == hubNamespace
                        || type.Namespace?.StartsWith(streamNamespacePrefix, StringComparison.Ordinal) == true));
        violations.AddRange(streamTypes.SelectMany(GetOperationLocationViolations));

        return violations;
    }

    private static IEnumerable<string> GetOperationLocationViolations(Type type)
    {
        var segments = PresentationArchitectureConvention.GetRelativeNamespace(type).Split('.');
        if (segments is not ["Features", _, "Streaming", _, "Requests" or "Notifications", _, ..])
        {
            return [$"{type.FullName} must reside in Requests/<Operation> or Notifications/<Event> within its stream."];
        }

        return PresentationArchitectureConvention.GetLocationViolations(type, segments);
    }

    private static Type? GetClientContract(Type hub)
    {
        for (var baseType = hub.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Hub<>))
            {
                return baseType.GetGenericArguments()[0];
            }
        }

        return null;
    }
}
