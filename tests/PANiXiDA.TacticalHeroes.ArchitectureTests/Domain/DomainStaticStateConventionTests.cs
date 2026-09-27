using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Domain;

public sealed class DomainStaticStateConventionTests
{
    private const BindingFlags StaticMemberFlags = BindingFlags.Static | BindingFlags.Public |
                                                   BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    [Fact(DisplayName = "Domain types should contain only immutable static state when authored members are declared")]
    public async Task DomainTypes_Should_ContainOnlyImmutableStaticState_When_AuthoredMembersAreDeclared()
    {
        var results = await ProductionSourceDocumentDiscovery.GetItemsAsync(GetStaticStateResultsAsync);
        var violations = results.SelectMany(result => result.Violations)
            .Distinct().Order(StringComparer.Ordinal).ToArray();

        Assert.NotEmpty(results);
        Assert.True(violations.Length == 0,
            $"Domain static state must be immutable:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Static state should reject shared mutation when authored member shapes vary")]
    [InlineData("public static int Value = 1;", false)]
    [InlineData("private static int Value = 1;", false)]
    [InlineData("internal static int Value = 1;", false)]
    [InlineData("public static readonly List<int> Values = [1];", false)]
    [InlineData("public static readonly ReadOnlyCollection<int> Values = new List<int>().AsReadOnly();", false)]
    [InlineData("public static readonly MutableElement Value = new();", false)]
    [InlineData("public static readonly ImmutableArray<MutableElement> Values = [];", false)]
    [InlineData("public static int Value { get; private set; }", false)]
    [InlineData("public static List<int> Values { get; } = [];", false)]
    [InlineData("public static event Action Changed;", false)]
    [InlineData("public static event Action Changed { add { } remove { } }", false)]
    [InlineData("public static int Value { get => 1; set { } }", false)]
    [InlineData("public static ref int Value => ref (new int[1])[0];", false)]
    [InlineData("private static class Cache { internal static readonly List<int> Values = []; }", false)]
    [InlineData("public const int Value = 1;", true)]
    [InlineData("public static readonly int Value = 1;", true)]
    [InlineData("public static readonly ImmutableElement Value = new(1);", true)]
    [InlineData("public static readonly ImmutableArray<ImmutableElement> Values = [];", true)]
    [InlineData("public static int Value { get; } = 1;", true)]
    [InlineData("public static ImmutableArray<int> Values { get; } = [];", true)]
    [InlineData("public static int Value => 1;", true)]
    [InlineData("public static ref readonly int Value => ref (new int[1])[0];", true)]
    [InlineData("public static MutableElement Create() => new();", true)]
    [InlineData("private static class Defaults { internal const int Value = 1; }", true)]
    public void StaticState_Should_RejectSharedMutation_When_AuthoredMemberShapesVary(string members, bool expected)
    {
        var source = $$"""
                       using System;
                       using System.Collections.Generic;
                       using System.Collections.Immutable;
                       using System.Collections.ObjectModel;

                       public sealed class Sample { {{members}} }
                       public sealed record ImmutableElement(int Value);
                       public sealed class MutableElement { public int Value { get; set; } }
                       """;
        var cancellationToken = TestContext.Current.CancellationToken;
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            assemblyName: $"StaticStateProbe_{Guid.NewGuid():N}", syntaxTrees: [tree], references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var emission = compilation.Emit(stream, cancellationToken: cancellationToken);
        Assert.True(emission.Success, string.Join(Environment.NewLine, emission.Diagnostics));
        var assembly = Assembly.Load(stream.ToArray());
        var membersToCheck = GetAuthoredStaticMembers(tree.GetRoot(cancellationToken), compilation.GetSemanticModel(tree));

        var results = membersToCheck.Select(member => CheckMember(assembly, member)).ToArray();

        Assert.NotEmpty(results);
        Assert.Equal(expected, results.All(result => result.Violations.Length == 0));
    }

    private static async Task<StaticStateResult[]> GetStaticStateResultsAsync(string repositoryRoot, Document document)
    {
        if (!document.Project.Name.EndsWith(".Domain", StringComparison.Ordinal))
        {
            return [];
        }

        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        if (root is null || semanticModel is null)
        {
            return [];
        }

        var assembly = ArchitectureDefinition.ProductionAssemblies.Single(candidate =>
            candidate.GetName().Name == document.Project.AssemblyName);

        return [.. GetAuthoredStaticMembers(root, semanticModel).Select(member => CheckMember(assembly, member))];
    }

    private static IEnumerable<StaticMember> GetAuthoredStaticMembers(SyntaxNode root, SemanticModel semanticModel)
    {
        foreach (var node in root.DescendantNodes())
        {
            var symbol = node switch
            {
                VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax } variable =>
                    semanticModel.GetDeclaredSymbol(variable),
                PropertyDeclarationSyntax property => semanticModel.GetDeclaredSymbol(property),
                EventDeclarationSyntax eventDeclaration => semanticModel.GetDeclaredSymbol(eventDeclaration),
                MethodDeclarationSyntax method => semanticModel.GetDeclaredSymbol(method),
                _ => null
            };

            if (symbol is not { IsStatic: true, ContainingType: { } owner })
            {
                continue;
            }

            var typeName = GetMetadataName(owner);
            yield return new StaticMember(typeName, symbol.MetadataName);

            if (symbol is IPropertySymbol)
            {
                foreach (var field in owner.GetMembers().OfType<IFieldSymbol>().Where(field =>
                             SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, symbol)))
                {
                    yield return new StaticMember(typeName, field.MetadataName);
                }
            }
        }
    }

    private static string GetMetadataName(INamedTypeSymbol type)
    {
        var prefix = type.ContainingType is { } owner
            ? GetMetadataName(owner) + "+"
            : type.ContainingNamespace.IsGlobalNamespace ? string.Empty : type.ContainingNamespace + ".";

        return prefix + type.MetadataName;
    }

    private static StaticStateResult CheckMember(Assembly assembly, StaticMember member)
    {
        var type = assembly.GetType(member.TypeName, throwOnError: true)!;
        var reflectedMembers = type.GetMember(member.Name, StaticMemberFlags);
        Assert.NotEmpty(reflectedMembers);

        return new StaticStateResult([.. reflectedMembers.SelectMany(GetViolations)]);
    }

    private static IEnumerable<string> GetViolations(MemberInfo member)
    {
        var name = $"{member.DeclaringType?.FullName}.{member.Name}";

        switch (member)
        {
            case FieldInfo { IsLiteral: false } field:
                if (!field.IsInitOnly)
                {
                    yield return $"{name} must be const or static readonly.";
                }

                foreach (var violation in DomainImmutableStateConvention.GetViolations(field.FieldType))
                {
                    yield return $"{name}: {violation}";
                }

                break;
            case PropertyInfo { SetMethod: not null }:
                yield return $"{name} must not declare a static setter.";
                break;
            case PropertyInfo { GetMethod: { } getter } when DomainImmutableStateConvention.ReturnsWritableReference(getter):
            case MethodInfo method when DomainImmutableStateConvention.ReturnsWritableReference(method):
                yield return $"{name} must not return a writable reference.";
                break;
            case EventInfo:
                yield return $"{name} must not declare a static event.";
                break;
        }
    }

    private sealed record StaticMember(string TypeName, string Name);

    private sealed record StaticStateResult(string[] Violations);
}
