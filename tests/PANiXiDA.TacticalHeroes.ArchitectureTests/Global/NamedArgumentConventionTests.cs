using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class NamedArgumentConventionTests
{
    [Fact(DisplayName = "Invocation and constructor arguments should be named when declared")]
    public void InvocationAndConstructorArguments_Should_BeNamed_When_Declared()
    {
        var arguments = NamedArgumentSourceDiscovery.GetArguments();
        var violations = arguments
            .Where(argument => !argument.IsNamed)
            .Select(argument =>
                $"{argument.RelativePath}:{argument.LineNumber}: argument " +
                $"'{argument.Argument}' passed to '{argument.Call}' must " +
                $"be named.")
            .ToArray();

        Assert.NotEmpty(arguments);
        Assert.True(
            violations.Length == 0,
            $"Positional argument violations: {violations.Length} total. "
                + $"Only the first 100 are shown:{Environment.NewLine}"
                + string.Join(
                    Environment.NewLine,
                    violations.Take(count: 100)));
    }
}

internal static class NamedArgumentSourceDiscovery
{
    private static readonly string[] SourceRootDirectoryNames = ["src"];

    private static readonly string[] ExcludedDirectoryNames =
    [
        "bin",
        "obj",
        "Generated",
        "Migrations"
    ];

    internal static NamedArgumentSource[] GetArguments()
    {
        var repositoryRoot = FindRepositoryRoot();

        return
        [
            .. GetProjectFiles(repositoryRoot)
                .SelectMany(projectFile =>
                    GetProjectArguments(
                        repositoryRoot,
                        projectFile))
                .OrderBy(
                    argument => argument.RelativePath,
                    StringComparer.Ordinal)
                .ThenBy(argument => argument.LineNumber)
        ];
    }

    private static IEnumerable<string> GetProjectFiles(
        string repositoryRoot)
    {
        return SourceRootDirectoryNames
            .Select(directoryName =>
                Path.Combine(repositoryRoot, directoryName))
            .Where(Directory.Exists)
            .SelectMany(sourceRoot =>
                Directory.EnumerateFiles(
                    sourceRoot,
                    "*.csproj",
                    SearchOption.AllDirectories))
            .Order(StringComparer.Ordinal);
    }

    private static IEnumerable<NamedArgumentSource> GetProjectArguments(
        string repositoryRoot,
        string projectFile)
    {
        var projectDirectory = Path.GetDirectoryName(projectFile)
            ?? throw new InvalidOperationException(
                $"Project '{projectFile}' does not have a directory.");

        return Directory
            .EnumerateFiles(
                projectDirectory,
                "*.cs",
                SearchOption.AllDirectories)
            .Where(IsSourceFile)
            .Order(StringComparer.Ordinal)
            .SelectMany(sourceFile =>
                GetSourceArguments(
                    repositoryRoot,
                    sourceFile));
    }

    private static IEnumerable<NamedArgumentSource> GetSourceArguments(
        string repositoryRoot,
        string sourceFile)
    {
        var root = CSharpSyntaxTree
            .ParseText(File.ReadAllText(sourceFile))
            .GetCompilationUnitRoot();
        var relativePath = Path.GetRelativePath(
            repositoryRoot,
            sourceFile);

        return root
            .DescendantNodes()
            .SelectMany(node =>
                GetNodeArguments(
                    relativePath,
                    node));
    }

    private static IEnumerable<NamedArgumentSource> GetNodeArguments(
        string relativePath,
        SyntaxNode node)
    {
        return node switch
        {
            InvocationExpressionSyntax invocation
                when !IsNameOf(invocation) =>
                GetArguments(
                    relativePath,
                    invocation.Expression.ToString(),
                    invocation.ArgumentList.Arguments),
            ObjectCreationExpressionSyntax creation
                when creation.ArgumentList is not null =>
                GetArguments(
                    relativePath,
                    $"new {creation.Type}",
                    creation.ArgumentList.Arguments),
            ImplicitObjectCreationExpressionSyntax creation =>
                GetArguments(
                    relativePath,
                    "new",
                    creation.ArgumentList.Arguments),
            ConstructorInitializerSyntax initializer =>
                GetArguments(
                    relativePath,
                    initializer.ThisOrBaseKeyword.ValueText,
                    initializer.ArgumentList.Arguments),
            PrimaryConstructorBaseTypeSyntax primaryConstructorBase =>
                GetArguments(
                    relativePath,
                    primaryConstructorBase.Type.ToString(),
                    primaryConstructorBase.ArgumentList.Arguments),
            _ => []
        };
    }

    private static IEnumerable<NamedArgumentSource> GetArguments(
        string relativePath,
        string call,
        SeparatedSyntaxList<ArgumentSyntax> arguments)
    {
        return arguments.Select(argument => new NamedArgumentSource(
            RelativePath: relativePath,
            LineNumber: argument
                .GetLocation()
                .GetLineSpan()
                .StartLinePosition.Line + 1,
            Call: call,
            Argument: argument.Expression.ToString(),
            IsNamed: argument.NameColon is not null));
    }

    private static bool IsNameOf(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression is IdentifierNameSyntax identifier &&
               string.Equals(
                   identifier.Identifier.ValueText,
                   "nameof",
                   StringComparison.Ordinal);
    }

    private static bool IsSourceFile(string path)
    {
        return !path
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

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (SourceRootDirectoryNames.All(directoryName =>
                Directory.Exists(Path.Combine(
                    directory.FullName,
                    directoryName))))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find repository root containing " +
            $"{string.Join(", ", SourceRootDirectoryNames)} directories.");
    }
}

internal sealed record NamedArgumentSource(
    string RelativePath,
    int LineNumber,
    string Call,
    string Argument,
    bool IsNamed);
