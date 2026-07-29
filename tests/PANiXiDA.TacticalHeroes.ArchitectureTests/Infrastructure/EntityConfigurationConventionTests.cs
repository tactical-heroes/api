using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.Infrastructure.Persistence.Ef.Write;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Infrastructure;

public sealed class EntityConfigurationConventionTests
{
    private static readonly string[] ExplicitStoreNamingMethods =
    [
        "HasColumnName",
        "ToTable",
        "ToView"
    ];

    [Fact(DisplayName = "Auditable entity configurations should reside in aggregate Write roots when declared")]
    public void AuditableEntityConfigurations_Should_ResideInAggregateWriteRoots_When_Declared()
    {
        var configurations = GetAuditableEntityConfigurations();
        var violations = configurations
            .SelectMany(type =>
                InfrastructurePersistenceConvention
                    .GetAggregateFeatureLocationViolations(
                        type,
                        "Write"))
            .ToArray();

        Assert.NotEmpty(configurations);
        Assert.True(
            violations.Length == 0,
            $"Auditable entity configuration location violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Entity type configurations should reside in aggregate Write roots when declared")]
    public void EntityTypeConfigurations_Should_ResideInAggregateWriteRoots_When_Declared()
    {
        var configurations = GetEntityTypeConfigurations();
        var violations = configurations
            .SelectMany(type =>
                InfrastructurePersistenceConvention
                    .GetAggregateFeatureLocationViolations(
                        type,
                        "Write"))
            .ToArray();

        Assert.NotEmpty(configurations);
        Assert.True(
            violations.Length == 0,
            $"Entity type configuration location violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Auditable entity configurations should avoid explicit store object names when declared")]
    public void AuditableEntityConfigurations_Should_AvoidExplicitStoreObjectNames_When_Declared()
    {
        var configurations = GetAuditableEntityConfigurations();
        var violations = configurations
            .SelectMany(GetExplicitStoreNamingViolations)
            .ToArray();

        Assert.NotEmpty(configurations);
        Assert.True(
            violations.Length == 0,
            $"Auditable entity configuration explicit naming violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Entity type configurations should avoid explicit store object names when declared")]
    public void EntityTypeConfigurations_Should_AvoidExplicitStoreObjectNames_When_Declared()
    {
        var configurations = GetEntityTypeConfigurations();
        var violations = configurations
            .SelectMany(GetExplicitStoreNamingViolations)
            .ToArray();

        Assert.NotEmpty(configurations);
        Assert.True(
            violations.Length == 0,
            $"Entity type configuration explicit naming violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static Type[] GetAuditableEntityConfigurations()
    {
        return InfrastructurePersistenceConvention
            .GetConcreteInfrastructureTypes(type =>
                InfrastructurePersistenceConvention
                    .GetClosedGenericBaseType(
                        type,
                        typeof(AuditableEntityConfiguration<>)) is not null);
    }

    private static Type[] GetEntityTypeConfigurations()
    {
        return InfrastructurePersistenceConvention
            .GetConcreteInfrastructureTypes(type =>
                InfrastructurePersistenceConvention
                    .GetClosedGenericInterface(
                        type,
                        typeof(IEntityTypeConfiguration<>)) is not null);
    }

    private static IEnumerable<string> GetExplicitStoreNamingViolations(
        Type configuration)
    {
        return InfrastructurePersistenceConvention
            .FindSourceFiles(configuration)
            .SelectMany(sourceFile =>
            {
                var syntaxRoot = CSharpSyntaxTree
                    .ParseText(File.ReadAllText(sourceFile))
                    .GetRoot();

                return syntaxRoot
                    .DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .Where(declaration => string.Equals(
                        declaration.Identifier.ValueText,
                        configuration.Name,
                        StringComparison.Ordinal))
                    .SelectMany(declaration => declaration
                        .DescendantNodes()
                        .OfType<InvocationExpressionSyntax>())
                    .Select(invocation => new
                    {
                        Invocation = invocation,
                        MethodName = GetInvokedMethodName(invocation)
                    })
                    .Where(candidate =>
                        candidate.MethodName is not null &&
                        ExplicitStoreNamingMethods.Contains(
                            candidate.MethodName,
                            StringComparer.Ordinal))
                    .Select(candidate =>
                        $"{configuration.FullName} explicitly calls " +
                        $"'{candidate.MethodName}' at " +
                        $"'{Path.GetFileName(sourceFile)}:" +
                        $"{candidate.Invocation.GetLocation()
                            .GetLineSpan()
                            .StartLinePosition.Line + 1}'.");
            });
    }

    private static string? GetInvokedMethodName(
        InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess =>
                memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier =>
                identifier.Identifier.ValueText,
            GenericNameSyntax genericName =>
                genericName.Identifier.ValueText,
            _ => null
        };
    }
}
