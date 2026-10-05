using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class NamedArgumentConventionTests
{
    [Fact(DisplayName = "Invocation and constructor arguments should be named when declared")]
    public async Task InvocationAndConstructorArguments_Should_BeNamed_When_Declared()
    {
        var arguments = await NamedArgumentSourceDiscovery.GetArgumentsAsync();
        var violations = arguments
            .Where(argument => argument.RequiresName && !argument.IsNamed)
            .Select(argument =>
                $"{argument.RelativePath}:{argument.LineNumber}: argument " +
                $"'{argument.Argument}' passed to '{argument.Call}' must be " +
                "named.")
            .ToArray();

        Assert.NotEmpty(arguments);
        Assert.True(
            violations.Length == 0,
            $"Named argument violations: {violations.Length} total. "
                + $"Only the first 100 are shown:{Environment.NewLine}"
                + string.Join(
                    Environment.NewLine,
                    violations.Take(count: 100)));
    }

    [Theory(DisplayName = "Invocation and constructor arguments should require names when source calls vary")]
    [InlineData("class C { void M(int value) {} void Test() => M(1); }", 1)]
    [InlineData("class C { void M(int first, int second) {} void Test() => M(1, 2); }", 2)]
    [InlineData("class C { void M(int first, int second, int third) {} void Test() => M(1, 2, 3); }", 3)]
    [InlineData("class C { void M(int first, int second) {} void Test() => M(first: 1, 2); }", 1)]
    [InlineData("class C { void M(int value) {} void Test() => M(value: 1); }", 0)]
    [InlineData("class C { public C(int value) {} C Test() => new C(1); }", 1)]
    [InlineData("class C { public C(int value) {} C Test() => new(1); }", 1)]
    [InlineData("class C { public C(int value) {} C Test() => new(value: 1); }", 0)]
    [InlineData("class C { C(int value) {} C() : this(1) {} }", 1)]
    [InlineData("class B { public B(int value) {} } class C : B { C() : base(1) {} }", 1)]
    [InlineData("class B(int value); class C(int value) : B(value);", 1)]
    [InlineData("class C { void M(int value, params int[] rest) {} void Test() => M(1, 2, 3); }", 0)]
    [InlineData("class C { bool Test() => string.Equals(\"first\", \"second\"); }", 0)]
    [InlineData("class C { string Test() => nameof(C); }", 0)]
    public void InvocationAndConstructorArguments_Should_RequireNames_When_SourceCallsVary(string source, int expectedViolations)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var compilation = CSharpCompilation.Create(
            assemblyName: "NamedArgumentProbe",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.DoesNotContain(compilation.GetDiagnostics(cancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        var arguments = NamedArgumentSourceDiscovery.GetArguments(
            tree.GetRoot(cancellationToken),
            compilation.GetSemanticModel(tree),
            "Sample.cs");
        var violations = arguments.Count(argument => argument.RequiresName && !argument.IsNamed);

        Assert.Equal(expectedViolations, violations);
    }
}

internal static class NamedArgumentSourceDiscovery
{
    internal static async Task<NamedArgumentSource[]> GetArgumentsAsync()
    {
        var arguments = await ProductionSourceDocumentDiscovery
            .GetItemsAsync(GetDocumentArgumentsAsync);

        return
        [
            .. arguments
                .Distinct()
                .OrderBy(
                    argument => argument.RelativePath,
                    StringComparer.Ordinal)
                .ThenBy(argument => argument.Position)
        ];
    }

    internal static NamedArgumentSource[] GetArguments(
        SyntaxNode root,
        SemanticModel semanticModel,
        string relativePath)
    {
        return
        [
            .. root
                .DescendantNodes()
                .SelectMany(node =>
                    GetNodeArguments(
                        relativePath,
                        semanticModel,
                        node))
        ];
    }

    private static async Task<NamedArgumentSource[]>
        GetDocumentArgumentsAsync(
            string repositoryRoot,
            Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var sourceFile = document.FilePath;

        if (root is null ||
            semanticModel is null ||
            sourceFile is null)
        {
            return [];
        }

        var relativePath = Path.GetRelativePath(
            repositoryRoot,
            sourceFile);

        return GetArguments(root, semanticModel, relativePath);
    }

    private static IEnumerable<NamedArgumentSource> GetNodeArguments(
        string relativePath,
        SemanticModel semanticModel,
        SyntaxNode node)
    {
        return node switch
        {
            InvocationExpressionSyntax invocation
                when !IsNameOf(invocation) =>
                GetArguments(
                    relativePath,
                    semanticModel,
                    invocation,
                    invocation.Expression.ToString(),
                    invocation.ArgumentList.Arguments),
            ObjectCreationExpressionSyntax creation
                when creation.ArgumentList is not null =>
                GetArguments(
                    relativePath,
                    semanticModel,
                    creation,
                    $"new {creation.Type}",
                    creation.ArgumentList.Arguments),
            ImplicitObjectCreationExpressionSyntax creation =>
                GetArguments(
                    relativePath,
                    semanticModel,
                    creation,
                    "new",
                    creation.ArgumentList.Arguments),
            ConstructorInitializerSyntax initializer =>
                GetArguments(
                    relativePath,
                    semanticModel,
                    initializer,
                    initializer.ThisOrBaseKeyword.ValueText,
                    initializer.ArgumentList.Arguments),
            PrimaryConstructorBaseTypeSyntax primaryConstructorBase =>
                GetArguments(
                    relativePath,
                    semanticModel,
                    primaryConstructorBase,
                    primaryConstructorBase.Type.ToString(),
                    primaryConstructorBase.ArgumentList.Arguments),
            _ => []
        };
    }

    private static IEnumerable<NamedArgumentSource> GetArguments(
        string relativePath,
        SemanticModel semanticModel,
        SyntaxNode callNode,
        string call,
        SeparatedSyntaxList<ArgumentSyntax> arguments)
    {
        var method = GetMethodSymbol(
            semanticModel,
            callNode);
        var callHasParamsParameter = method?.Parameters
            .Any(parameter => parameter.IsParams) == true;
        var callIsSystemString =
            method?.ContainingType.SpecialType ==
            SpecialType.System_String;
        return arguments.Select(argument =>
        {
            var requiresName =
                !callHasParamsParameter &&
                !callIsSystemString;

            return new NamedArgumentSource(
                RelativePath: relativePath,
                LineNumber: argument
                    .GetLocation()
                    .GetLineSpan()
                    .StartLinePosition.Line + 1,
                Position: argument.SpanStart,
                Call: call,
                Argument: argument.Expression.ToString(),
                IsNamed: argument.NameColon is not null,
                RequiresName: requiresName);
        });
    }

    private static IMethodSymbol? GetMethodSymbol(
        SemanticModel semanticModel,
        SyntaxNode callNode)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(callNode);

        return symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols
                .OfType<IMethodSymbol>()
                .SingleOrDefault();
    }

    private static bool IsNameOf(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression is IdentifierNameSyntax identifier &&
               string.Equals(
                   identifier.Identifier.ValueText,
                   "nameof",
                   StringComparison.Ordinal);
    }
}

internal sealed record NamedArgumentSource(
    string RelativePath,
    int LineNumber,
    int Position,
    string Call,
    string Argument,
    bool IsNamed,
    bool RequiresName);
