using System.Text.RegularExpressions;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Tests;

public sealed class TestDisplayNameConventionTests
{
    private const string SourceDirectoryName = "src";
    private const string TestsDirectoryName = "tests";

    private static readonly Regex TestAttributePattern = new(
        @"^[ \t]*(?<source>\[(?:Fact|Theory)(?:Attribute)?\b" +
        @"(?:(?:[^\[\]""']+|""(?:\\.|[^""\\])*""|" +
        @"'(?:\\.|[^'\\])*'|\[(?<bracketDepth>)|" +
        @"\](?<-bracketDepth>))*)" +
        @"(?(bracketDepth)(?!))\])",
        RegexOptions.CultureInvariant | RegexOptions.Multiline);

    private static readonly Regex DisplayNamePattern = new(
        @"DisplayName\s*=\s*""(?<value>(?:\\.|[^""\\])*)""",
        RegexOptions.CultureInvariant);

    private static readonly Regex EnglishBehaviorDescriptionPattern = new(
        @"^[A-Za-z][\x20-\x5B\x5D-\x7E]* should " +
        @"[A-Za-z0-9][\x20-\x5B\x5D-\x7E]*$",
        RegexOptions.CultureInvariant);

    [Fact(DisplayName = "Fact and Theory tests should declare display names")]
    public void FactsAndTheories_Should_DeclareDisplayName()
    {
        var testAttributes = GetTestAttributes();

        Assert.NotEmpty(testAttributes);

        var missingDisplayNames = testAttributes
            .Where(testAttribute =>
                !DisplayNamePattern.IsMatch(testAttribute.Source))
            .Select(FormatViolation)
            .ToArray();

        Assert.True(
            missingDisplayNames.Length == 0,
            $"Fact and Theory attributes without DisplayName:{Environment.NewLine}" +
            string.Join(Environment.NewLine, missingDisplayNames));
    }

    [Fact(DisplayName = "Fact and Theory display names should use English behavior descriptions")]
    public void DisplayNames_Should_UseEnglishBehaviorDescriptions()
    {
        var testAttributes = GetTestAttributes();

        Assert.NotEmpty(testAttributes);

        var invalidDisplayNames = testAttributes
            .Select(testAttribute => new
            {
                TestAttribute = testAttribute,
                DisplayNameMatch = DisplayNamePattern.Match(testAttribute.Source)
            })
            .Where(item => item.DisplayNameMatch.Success)
            .Select(item => new
            {
                item.TestAttribute,
                DisplayName = item.DisplayNameMatch.Groups["value"].Value
            })
            .Where(item =>
                !EnglishBehaviorDescriptionPattern.IsMatch(item.DisplayName) ||
                !string.Equals(
                    item.DisplayName,
                    item.DisplayName.Trim(),
                    StringComparison.Ordinal))
            .Select(item =>
                $"{FormatLocation(item.TestAttribute)}: \"{item.DisplayName}\"")
            .ToArray();

        Assert.True(
            invalidDisplayNames.Length == 0,
            $"DisplayName must be an English ASCII description in " +
            $"'<subject> should <behavior>' format:{Environment.NewLine}" +
            string.Join(Environment.NewLine, invalidDisplayNames));
    }

    private static TestAttribute[] GetTestAttributes()
    {
        var repositoryRoot = FindRepositoryRoot();
        var testsRoot = Path.Combine(repositoryRoot, TestsDirectoryName);

        return
        [
            .. Directory
                .EnumerateFiles(
                    testsRoot,
                    "*.cs",
                    SearchOption.AllDirectories)
                .Where(IsSourceFile)
                .OrderBy(path => path, StringComparer.Ordinal)
                .SelectMany(path =>
                    GetTestAttributes(repositoryRoot, path))
        ];
    }

    private static IEnumerable<TestAttribute> GetTestAttributes(
        string repositoryRoot,
        string path)
    {
        var source = File.ReadAllText(path);

        return TestAttributePattern
            .Matches(source)
            .Select(match => new TestAttribute(
                RelativePath: Path.GetRelativePath(repositoryRoot, path),
                LineNumber: GetLineNumber(source, match.Index),
                Source: match.Groups["source"].Value));
    }

    private static int GetLineNumber(string source, int index)
    {
        var lineNumber = 1;

        for (var characterIndex = 0;
             characterIndex < index;
             characterIndex++)
        {
            if (source[characterIndex] == '\n')
            {
                lineNumber++;
            }
        }

        return lineNumber;
    }

    private static bool IsSourceFile(string path)
    {
        return !path
            .Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries)
            .Any(segment =>
                string.Equals(segment, "bin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(segment, "obj", StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatViolation(TestAttribute testAttribute)
    {
        return $"{FormatLocation(testAttribute)}: {testAttribute.Source.Trim()}";
    }

    private static string FormatLocation(TestAttribute testAttribute)
    {
        return $"{testAttribute.RelativePath}:{testAttribute.LineNumber}";
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(
                    directory.FullName,
                    SourceDirectoryName)) &&
                Directory.Exists(Path.Combine(
                    directory.FullName,
                    TestsDirectoryName)))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find repository root containing " +
            $"'{SourceDirectoryName}' and '{TestsDirectoryName}' directories.");
    }

    private sealed record TestAttribute(
        string RelativePath,
        int LineNumber,
        string Source);
}
