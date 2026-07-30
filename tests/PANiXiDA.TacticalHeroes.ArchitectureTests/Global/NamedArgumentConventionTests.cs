using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class NamedArgumentConventionTests
{
    [Fact(DisplayName = "Invocation and constructor arguments should be named when ambiguous")]
    public async Task InvocationAndConstructorArguments_Should_BeNamed_When_Ambiguous()
    {
        var arguments = await NamedArgumentSourceDiscovery.GetArgumentsAsync();
        var violations = arguments
            .Where(argument => argument.RequiresName && !argument.IsNamed)
            .Select(argument =>
                $"{argument.RelativePath}:{argument.LineNumber}: argument " +
                $"'{argument.Argument}' passed to '{argument.Call}' must be " +
                $"named because {argument.Requirement}.")
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
}

internal static class NamedArgumentSourceDiscovery
{
    private const int NamedArgumentsRequiredFromCount = 3;
    private const string SourceRootDirectoryName = "src";

    private static readonly string[] ExcludedDirectoryNames =
    [
        "bin",
        "obj",
        "Generated",
        "Migrations"
    ];

    internal static async Task<NamedArgumentSource[]> GetArgumentsAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var solutionPath = Directory
            .EnumerateFiles(
                repositoryRoot,
                "*.slnx",
                SearchOption.TopDirectoryOnly)
            .Single();
        using var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionPath);
        var arguments = new List<NamedArgumentSource>();

        foreach (var project in solution.Projects
                     .Where(project =>
                         IsSourceProject(
                             repositoryRoot,
                             project.FilePath))
                     .OrderBy(
                         project => project.FilePath,
                         StringComparer.Ordinal))
        {
            foreach (var document in project.Documents
                         .Where(document =>
                             IsSourceFile(
                                 repositoryRoot,
                                 document.FilePath))
                         .OrderBy(
                             document => document.FilePath,
                             StringComparer.Ordinal))
            {
                arguments.AddRange(
                    await GetDocumentArgumentsAsync(
                        repositoryRoot,
                        document));
            }
        }

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
        var allArgumentsRequireNames =
            arguments.Count >= NamedArgumentsRequiredFromCount;

        return arguments.Select(argument =>
        {
            var isAmbiguousLiteral = IsAmbiguousLiteral(
                argument.Expression);
            var requiresName =
                !callHasParamsParameter &&
                (isAmbiguousLiteral || allArgumentsRequireNames);
            var requirement = isAmbiguousLiteral
                ? "null, default and boolean literals are ambiguous"
                : $"the call declares {arguments.Count} arguments";

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
                RequiresName: requiresName,
                Requirement: requirement);
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

    private static bool IsAmbiguousLiteral(ExpressionSyntax expression)
    {
        var unwrappedExpression = UnwrapExpression(expression);

        return unwrappedExpression.IsKind(
                   SyntaxKind.NullLiteralExpression) ||
               unwrappedExpression.IsKind(
                   SyntaxKind.DefaultLiteralExpression) ||
               unwrappedExpression.IsKind(
                   SyntaxKind.TrueLiteralExpression) ||
               unwrappedExpression.IsKind(
                   SyntaxKind.FalseLiteralExpression) ||
               unwrappedExpression is DefaultExpressionSyntax;
    }

    private static ExpressionSyntax UnwrapExpression(
        ExpressionSyntax expression)
    {
        return expression switch
        {
            ParenthesizedExpressionSyntax parenthesized =>
                UnwrapExpression(parenthesized.Expression),
            CastExpressionSyntax cast =>
                UnwrapExpression(cast.Expression),
            PostfixUnaryExpressionSyntax postfix
                when postfix.IsKind(
                    SyntaxKind.SuppressNullableWarningExpression) =>
                UnwrapExpression(postfix.Operand),
            _ => expression
        };
    }

    private static bool IsNameOf(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression is IdentifierNameSyntax identifier &&
               string.Equals(
                   identifier.Identifier.ValueText,
                   "nameof",
                   StringComparison.Ordinal);
    }

    private static bool IsSourceProject(
        string repositoryRoot,
        string? projectFile)
    {
        return projectFile is not null &&
               IsWithinSourceRoot(
                   repositoryRoot,
                   projectFile);
    }

    private static bool IsSourceFile(
        string repositoryRoot,
        string? sourceFile)
    {
        return sourceFile is not null &&
               IsWithinSourceRoot(
                   repositoryRoot,
                   sourceFile) &&
               !sourceFile
                   .Split(
                       [
                           Path.DirectorySeparatorChar,
                           Path.AltDirectorySeparatorChar
                       ],
                       StringSplitOptions.RemoveEmptyEntries)
                   .Any(segment =>
                       ExcludedDirectoryNames.Contains(
                           segment,
                           StringComparer.OrdinalIgnoreCase));
    }

    private static bool IsWithinSourceRoot(
        string repositoryRoot,
        string path)
    {
        var sourceRoot = Path.Combine(
                repositoryRoot,
                SourceRootDirectoryName)
            + Path.DirectorySeparatorChar;

        return Path.GetFullPath(path)
            .StartsWith(
                sourceRoot,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(
                    directory.FullName,
                    SourceRootDirectoryName)) &&
                Directory.EnumerateFiles(
                        directory.FullName,
                        "*.slnx",
                        SearchOption.TopDirectoryOnly)
                    .Any())
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find repository root containing " +
            $"'{SourceRootDirectoryName}' and a solution file.");
    }
}

internal sealed record NamedArgumentSource(
    string RelativePath,
    int LineNumber,
    int Position,
    string Call,
    string Argument,
    bool IsNamed,
    bool RequiresName,
    string Requirement);
