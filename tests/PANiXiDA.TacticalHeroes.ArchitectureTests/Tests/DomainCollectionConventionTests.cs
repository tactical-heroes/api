using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Tests;

public sealed class DomainCollectionConventionTests
{
    [Theory(DisplayName = "Collection returns should identify protected storage when expressions use different collection types")]
    [InlineData("new List<int>()", false)]
    [InlineData("new HashSet<int>()", false)]
    [InlineData("new Dictionary<int, int>()", false)]
    [InlineData("new Queue<int>()", false)]
    [InlineData("new Stack<int>()", false)]
    [InlineData("new int[1]", false)]
    [InlineData("(IReadOnlyCollection<int>)new List<int>()", false)]
    [InlineData("values", false)]
    [InlineData("AsReadOnly()", false)]
    [InlineData("ImmutableArray.Create(1).ToBuilder()", false)]
    [InlineData("(MutableView)ImmutableArray.Create(1)", false)]
    [InlineData("choice ? new List<int>().AsReadOnly() : new List<int>()", false)]
    [InlineData("new List<int>().AsReadOnly()", true)]
    [InlineData("new HashSet<int>().AsReadOnly()", true)]
    [InlineData("new Dictionary<int, int>().AsReadOnly()", true)]
    [InlineData("Array.AsReadOnly(new int[1])", true)]
    [InlineData("Array.AsReadOnly(new Queue<int>().ToArray())", true)]
    [InlineData("ImmutableArray.Create(1)", true)]
    [InlineData("ImmutableDictionary<int, int>.Empty", true)]
    [InlineData("new HashSet<int>().ToFrozenSet()", true)]
    [InlineData("new Dictionary<int, int>().ToFrozenDictionary()", true)]
    [InlineData("choice ? new List<int>().AsReadOnly() : Array.AsReadOnly(new int[1])", true)]
    [InlineData("null", true)]
    [InlineData("default(IReadOnlyCollection<int>)", true)]
    [InlineData("choice ? new List<int>().AsReadOnly() : null", true)]
    [InlineData("choice ? new List<int>() : null", false)]
    [InlineData("(IReadOnlyCollection<int>)null ?? new List<int>().AsReadOnly()", true)]
    [InlineData("(IReadOnlyCollection<int>)null ?? new List<int>()", false)]
    public void CollectionReturns_Should_IdentifyProtectedStorage_When_ExpressionsUseDifferentCollectionTypes(
        string expression,
        bool expected)
    {
        var source = $$"""
                       using System;
                       using System.Collections;
                       using System.Collections.Generic;
                       using System.Collections.Frozen;
                       using System.Collections.Immutable;

                       public static class Sample
                       {
                           public static IEnumerable GetItems(bool choice, IReadOnlyList<int> values) => {{expression}};

                           private static IEnumerable AsReadOnly() => new List<int>();
                       }

                       public sealed class MutableView : List<int>
                       {
                           public static explicit operator MutableView(ImmutableArray<int> values) => new();
                       }
                       """;
        var cancellationToken = TestContext.Current.CancellationToken;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            assemblyName: "CollectionConventionProbe",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var returnExpression = syntaxTree.GetRoot(cancellationToken)
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.ValueText == "GetItems")
            .ExpressionBody!.Expression;
        var result = DomainCollectionConvention.IsProtectedCollection(
            compilation.GetSemanticModel(syntaxTree).GetOperation(returnExpression, cancellationToken),
            compilation);

        Assert.DoesNotContain(compilation.GetDiagnostics(cancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Equal(expected, result);
    }
}
