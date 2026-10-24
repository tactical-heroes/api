using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class CancellationTokenConventionTests
{
    [Fact(DisplayName = "Cancellation token parameters should be last when signatures are declared")]
    public async Task CancellationTokenParameters_Should_BeLast_When_SignaturesAreDeclared()
    {
        var analysis = await CancellationTokenSourceDiscovery.GetAnalysisAsync();
        var violations = analysis.Parameters
            .Where(parameter => !parameter.IsLast && !parameter.HasExternallyDefinedSignature)
            .Select(parameter =>
                $"{parameter.RelativePath}:{parameter.LineNumber}: "
                    + $"cancellation token parameter '{parameter.Name}' must be last.")
            .ToArray();

        Assert.NotEmpty(analysis.Parameters);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Cancellation token arguments should be last when calls are written")]
    public async Task CancellationTokenArguments_Should_BeLast_When_CallsAreWritten()
    {
        var analysis = await CancellationTokenSourceDiscovery.GetAnalysisAsync();
        var violations = analysis.Invocations
            .Where(invocation => !invocation.PassesTokenLast)
            .Select(invocation =>
                $"{invocation.RelativePath}:{invocation.LineNumber}: "
                    + $"'{invocation.Expression}' must pass the cancellation token last.")
            .ToArray();

        Assert.NotEmpty(analysis.Invocations);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Cancellation token ordering should detect misplaced tokens when source shapes vary")]
    [InlineData("void Run(int value, Cancellation token) { }", false, false)]
    [InlineData("void Run(Cancellation token, int value) { }", true, false)]
    [InlineData("void Run(Cancellation token, int value = 0) { }", true, false)]
    [InlineData("public Sample(Cancellation token, int value) { }", true, false)]
    [InlineData("public class Inner(Cancellation token, int value);", true, false)]
    [InlineData("void Run() { void Local(Cancellation token, int value) { } }", true, false)]
    [InlineData("public delegate void Callback(Cancellation token, int value);", true, false)]
    [InlineData("void Run() { System.Action<Cancellation, int> action = (token, value) => { }; }", true, false)]
    [InlineData("void Run(Cancellation token) { Execute(1, token); }", false, false)]
    [InlineData("void Run(Cancellation token) { Execute(token: token, value: 1); }", false, true)]
    [InlineData("void Run(Cancellation token) { Execute(value: 1, token: token); }", false, false)]
    [InlineData(
        "class Inner { public Inner(int value, Cancellation token) { } } "
            + "void Run(Cancellation token) { _ = new Inner(token: token, value: 1); }",
        false,
        true)]
    [InlineData("public struct CancellationToken { } void Run(CancellationToken token, int value) { }", false, false)]
    [InlineData("interface IContract { void Run(Cancellation token, int value); }", true, false)]
    [InlineData(
        "void Run(Cancellation token) { Many(token, 1, 2); } "
            + "void Many(Cancellation token, params int[] values) { }",
        true,
        true)]
    [InlineData("void Run(Cancellation token) { Execute(token: token!, value: 1); }", false, true)]
    public void CancellationTokenOrdering_Should_DetectMisplacedTokens_When_SourceShapesVary(
        string member, bool parameterViolation, bool argumentViolation)
    {
        var source = $$"""
                       using Cancellation = System.Threading.CancellationToken;
                       public class Sample
                       {
                           void Execute(int value, Cancellation token) { }
                           {{member}}
                       }
                       """;
        var ct = TestContext.Current.CancellationToken;
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: ct);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            assemblyName: "CancellationTokenOrderProbe",
            syntaxTrees: [tree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.DoesNotContain(
            compilation.GetDiagnostics(ct), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        var root = tree.GetRoot(ct);
        var model = compilation.GetSemanticModel(tree);

        var parameters = CancellationTokenSourceDiscovery.GetParameters("Probe.cs", model, root);
        var invocations = CancellationTokenSourceDiscovery.GetInvocations("Probe.cs", model, root);

        Assert.Equal(parameterViolation, parameters.Any(item => !item.IsLast && !item.HasExternallyDefinedSignature));
        Assert.Equal(argumentViolation, invocations.Any(item => !item.PassesTokenLast));
    }

    [Fact(DisplayName = "Cancellation token sentinels should not be used when declared")]
    public async Task CancellationTokenSentinels_Should_NotBeUsed_When_Declared()
    {
        var analysis = await CancellationTokenSourceDiscovery.GetAnalysisAsync();
        var violations = analysis.Sentinels
            .Select(sentinel =>
                $"{sentinel.RelativePath}:{sentinel.LineNumber}: "
                    + $"'{sentinel.Expression}' must be replaced with an actual "
                    + $"cancellation token.")
            .ToArray();

        Assert.NotEmpty(analysis.Parameters);
        Assert.True(
            violations.Length == 0,
            $"Cancellation token sentinel violations: "
                + $"{violations.Length} total.{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Cancellation token parameters should be used when declared")]
    public async Task CancellationTokenParameters_Should_BeUsed_When_Declared()
    {
        var analysis = await CancellationTokenSourceDiscovery.GetAnalysisAsync();
        var violations = analysis.Parameters
            .Where(parameter =>
                parameter.HasImplementation
                    && !parameter.HasExternallyDefinedSignature
                    && !parameter.IsUsed)
            .Select(parameter =>
                $"{parameter.RelativePath}:{parameter.LineNumber}: "
                    + $"cancellation token parameter '{parameter.Name}' is unused "
                    + $"and should be removed.")
            .ToArray();

        Assert.NotEmpty(analysis.Parameters);
        Assert.True(
            violations.Length == 0,
            $"Unused cancellation token parameters: "
                + $"{violations.Length} total.{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Cancellation tokens should be forwarded when available")]
    public async Task CancellationTokens_Should_BeForwarded_When_Available()
    {
        var analysis = await CancellationTokenSourceDiscovery.GetAnalysisAsync();
        var violations = analysis.Invocations
            .Where(invocation =>
                invocation.HasAvailableToken
                    && !invocation.PassesAllTokens)
            .Select(invocation =>
                $"{invocation.RelativePath}:{invocation.LineNumber}: "
                    + $"'{invocation.Expression}' should receive the available "
                    + $"cancellation token.")
            .ToArray();

        Assert.NotEmpty(analysis.Invocations);
        Assert.True(
            violations.Length == 0,
            $"Cancellation token forwarding violations: "
                + $"{violations.Length} total.{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Cancellation tokens should be available when cancellable operation is invoked")]
    public async Task CancellationTokens_Should_BeAvailable_When_CancellableOperationIsInvoked()
    {
        var analysis = await CancellationTokenSourceDiscovery.GetAnalysisAsync();
        var violations = analysis.Invocations
            .Where(invocation =>
                IsProductionSource(invocation.RelativePath)
                    && !invocation.HasAvailableToken
                    && !invocation.PassesAllTokens)
            .Select(invocation =>
                $"{invocation.RelativePath}:{invocation.LineNumber}: "
                    + $"'{invocation.Expression}' supports cancellation, but no "
                    + $"token is available. Add a CancellationToken parameter and "
                    + $"propagate it from the entry point.")
            .ToArray();

        Assert.NotEmpty(analysis.Invocations);
        Assert.True(
            violations.Length == 0,
            $"Missing cancellation token sources: "
                + $"{violations.Length} total.{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    private static bool IsProductionSource(string relativePath)
    {
        return relativePath.StartsWith(
            $"src{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase);
    }
}

internal static class CancellationTokenSourceDiscovery
{
    private const string CancellationTokenTypeName =
        "System.Threading.CancellationToken";

    private static readonly Lazy<Task<CancellationTokenAnalysis>> s_analysis =
        new(CreateAnalysisAsync);

    internal static Task<CancellationTokenAnalysis> GetAnalysisAsync()
    {
        return s_analysis.Value;
    }

    internal static CancellationTokenParameter[] GetParameters(
        string relativePath,
        SemanticModel semanticModel,
        SyntaxNode root)
    {
        return
        [
            .. root
                .DescendantNodes()
                .OfType<ParameterSyntax>()
                .Select(parameter => GetParameter(
                    relativePath,
                    semanticModel,
                    parameter))
                .OfType<CancellationTokenParameter>()
        ];
    }

    internal static CancellationTokenInvocation[] GetInvocations(
        string relativePath,
        SemanticModel semanticModel,
        SyntaxNode root)
    {
        return
        [
            .. root
                .DescendantNodes()
                .Where(node => node is InvocationExpressionSyntax or BaseObjectCreationExpressionSyntax
                    or ConstructorInitializerSyntax)
                .Select(invocation => GetInvocation(
                    relativePath,
                    semanticModel,
                    invocation))
                .OfType<CancellationTokenInvocation>()
        ];
    }

    private static async Task<CancellationTokenAnalysis> CreateAnalysisAsync()
    {
        var documents = await ProductionSourceDocumentDiscovery
            .GetAuthorItemsAsync(GetDocumentAnalysisAsync);

        return new CancellationTokenAnalysis(
            Parameters:
            [
                .. documents
                    .SelectMany(document => document.Parameters)
                    .OrderBy(
                        parameter => parameter.RelativePath,
                        StringComparer.Ordinal)
                    .ThenBy(parameter => parameter.Position)
            ],
            Sentinels:
            [
                .. documents
                    .SelectMany(document => document.Sentinels)
                    .OrderBy(
                        sentinel => sentinel.RelativePath,
                        StringComparer.Ordinal)
                    .ThenBy(sentinel => sentinel.Position)
            ],
            Invocations:
            [
                .. documents
                    .SelectMany(document => document.Invocations)
                    .OrderBy(
                        invocation => invocation.RelativePath,
                        StringComparer.Ordinal)
                    .ThenBy(invocation => invocation.Position)
            ]);
    }

    private static async Task<CancellationTokenDocumentAnalysis[]>
        GetDocumentAnalysisAsync(
            string repositoryRoot,
            Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var sourceFile = document.FilePath;

        if (root is null
            || semanticModel is null
            || sourceFile is null)
        {
            return [];
        }

        var relativePath = Path.GetRelativePath(
            repositoryRoot,
            sourceFile);

        return
        [
            new CancellationTokenDocumentAnalysis(
                Parameters: GetParameters(
                    relativePath,
                    semanticModel,
                    root),
                Sentinels: GetSentinels(
                    relativePath,
                    semanticModel,
                    root),
                Invocations: GetInvocations(
                    relativePath,
                    semanticModel,
                    root))
        ];
    }

    private static CancellationTokenParameter? GetParameter(
        string relativePath,
        SemanticModel semanticModel,
        ParameterSyntax parameter)
    {
        if (semanticModel.GetDeclaredSymbol(parameter) is not
                IParameterSymbol parameterSymbol
            || !IsCancellationToken(parameterSymbol.Type))
        {
            return null;
        }

        var callable = GetCallable(parameter);
        var method = parameterSymbol.ContainingSymbol as IMethodSymbol;

        return new CancellationTokenParameter(
            RelativePath: relativePath,
            LineNumber: GetLineNumber(parameter),
            Position: parameter.SpanStart,
            Name: parameterSymbol.Name,
            HasImplementation: callable is not null
                && HasImplementation(callable),
            HasExternallyDefinedSignature:
                method is not null
                    && HasExternallyDefinedSignature(method),
            IsLast: method is null || parameterSymbol.Ordinal == method.Parameters.Length - 1,
            IsUsed: callable is not null
                && callable
                    .DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Any(identifier => SymbolEqualityComparer.Default.Equals(
                        semanticModel.GetSymbolInfo(identifier).Symbol,
                        parameterSymbol)));
    }

    private static CancellationTokenSentinel[] GetSentinels(
        string relativePath,
        SemanticModel semanticModel,
        SyntaxNode root)
    {
        return
        [
            .. root
                .DescendantNodes()
                .OfType<ExpressionSyntax>()
                .Where(expression => IsSentinel(
                    semanticModel,
                    expression))
                .Select(expression => new CancellationTokenSentinel(
                    RelativePath: relativePath,
                    LineNumber: GetLineNumber(expression),
                    Position: expression.SpanStart,
                    Expression: expression.ToString()))
        ];
    }

    private static CancellationTokenInvocation? GetInvocation(
        string relativePath,
        SemanticModel semanticModel,
        SyntaxNode invocation)
    {
        var operation = semanticModel.GetOperation(invocation);
        var method = operation switch
        {
            IInvocationOperation call => call.TargetMethod,
            IObjectCreationOperation creation => creation.Constructor,
            _ => null
        };
        if (method is null)
        {
            return null;
        }

        var tokenParameters = method.Parameters
            .Where(parameter => IsCancellationToken(parameter.Type))
            .ToArray();

        if (tokenParameters.Length == 0)
        {
            return null;
        }

        var arguments = operation switch
        {
            IInvocationOperation call => call.Arguments,
            IObjectCreationOperation creation => creation.Arguments,
            _ => []
        };
        var sourceArguments = invocation.ChildNodes().OfType<ArgumentListSyntax>()
            .SelectMany(list => list.Arguments)
            .ToArray();
        var explicitArguments = arguments
            .Where(argument => sourceArguments.Any(source => source.Span.Contains(argument.Syntax.Span)))
            .ToArray();
        var explicitlyPassedParameters = explicitArguments
            .OrderBy(argument => argument.Syntax.SpanStart)
            .Select(argument => argument.Parameter)
            .OfType<IParameterSymbol>()
            .ToArray();
        var lastArgument = sourceArguments.LastOrDefault();

        return new CancellationTokenInvocation(
            RelativePath: relativePath,
            LineNumber: GetLineNumber(invocation),
            Position: invocation.SpanStart,
            Expression: invocation is InvocationExpressionSyntax callExpression
                ? callExpression.Expression.ToString()
                : method.ToDisplayString(),
            HasAvailableToken: semanticModel
                .LookupSymbols(invocation.SpanStart)
                .Any(IsCancellationTokenValue),
            PassesTokenLast: !explicitArguments.Any(argument => argument.Parameter is { } parameter
                && IsCancellationToken(parameter.Type)
                && lastArgument is not null
                && argument.Syntax.SpanStart < lastArgument.SpanStart),
            PassesAllTokens: tokenParameters.All(parameter =>
                explicitlyPassedParameters.Any(passedParameter =>
                    SymbolEqualityComparer.Default.Equals(
                        passedParameter,
                        parameter))));
    }

    private static bool IsSentinel(
        SemanticModel semanticModel,
        ExpressionSyntax expression)
    {
        if (expression is MemberAccessExpressionSyntax memberAccess
            && semanticModel.GetSymbolInfo(memberAccess).Symbol is
                IPropertySymbol property
            && property.IsStatic
            && string.Equals(
                property.Name,
                "None",
                StringComparison.Ordinal)
            && IsCancellationToken(property.ContainingType))
        {
            return true;
        }

        return (expression.IsKind(SyntaxKind.DefaultLiteralExpression)
            || expression is DefaultExpressionSyntax)
            && IsCancellationToken(
                semanticModel.GetTypeInfo(expression).ConvertedType);
    }

    private static SyntaxNode? GetCallable(ParameterSyntax parameter)
    {
        return parameter.Ancestors().FirstOrDefault(node =>
            node is BaseMethodDeclarationSyntax or
                LocalFunctionStatementSyntax or
                ParenthesizedLambdaExpressionSyntax or
                SimpleLambdaExpressionSyntax or
                AnonymousMethodExpressionSyntax);
    }

    private static bool HasImplementation(SyntaxNode callable)
    {
        return callable switch
        {
            BaseMethodDeclarationSyntax method =>
                method.Body is not null
                    || method.ExpressionBody is not null,
            LocalFunctionStatementSyntax localFunction =>
                localFunction.Body is not null
                    || localFunction.ExpressionBody is not null,
            AnonymousFunctionExpressionSyntax => true,
            _ => false
        };
    }

    private static bool HasExternallyDefinedSignature(IMethodSymbol method)
    {
        if (method.IsOverride
            || method.ExplicitInterfaceImplementations.Length > 0)
        {
            return true;
        }

        return method.ContainingType?.AllInterfaces
            .SelectMany(interfaceType => interfaceType.GetMembers())
            .Any(member => SymbolEqualityComparer.Default.Equals(
                method.ContainingType.FindImplementationForInterfaceMember(
                    member),
                method)) == true;
    }

    private static bool IsCancellationTokenValue(ISymbol symbol)
    {
        return symbol switch
        {
            IParameterSymbol parameter =>
                IsCancellationToken(parameter.Type),
            ILocalSymbol local =>
                IsCancellationToken(local.Type),
            IFieldSymbol field =>
                IsCancellationToken(field.Type),
            IPropertySymbol property =>
                IsCancellationToken(property.Type),
            _ => false
        };
    }

    private static bool IsCancellationToken(ITypeSymbol? type)
    {
        return string.Equals(
            type?.ToDisplayString(),
            CancellationTokenTypeName,
            StringComparison.Ordinal);
    }

    private static int GetLineNumber(SyntaxNode node)
    {
        return node
            .GetLocation()
            .GetLineSpan()
            .StartLinePosition.Line
            + 1;
    }
}

internal sealed record CancellationTokenAnalysis(
    CancellationTokenParameter[] Parameters,
    CancellationTokenSentinel[] Sentinels,
    CancellationTokenInvocation[] Invocations);

internal sealed record CancellationTokenDocumentAnalysis(
    CancellationTokenParameter[] Parameters,
    CancellationTokenSentinel[] Sentinels,
    CancellationTokenInvocation[] Invocations);

internal sealed record CancellationTokenParameter(
    string RelativePath,
    int LineNumber,
    int Position,
    string Name,
    bool HasImplementation,
    bool HasExternallyDefinedSignature,
    bool IsLast,
    bool IsUsed);

internal sealed record CancellationTokenSentinel(
    string RelativePath,
    int LineNumber,
    int Position,
    string Expression);

internal sealed record CancellationTokenInvocation(
    string RelativePath,
    int LineNumber,
    int Position,
    string Expression,
    bool HasAvailableToken,
    bool PassesTokenLast,
    bool PassesAllTokens);
