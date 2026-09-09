using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class SourceFormattingConventionTests
{
    [Fact(DisplayName = "Closing braces should not follow blank lines when source code is declared")]
    public async Task ClosingBraces_Should_NotFollowBlankLines_When_SourceCodeIsDeclared()
    {
        var violations = await ProductionSourceDocumentDiscovery
            .GetAuthorItemsAsync(GetClosingBraceViolationsAsync);

        Assert.True(
            violations.Length == 0,
            $"Remove blank lines immediately before closing braces:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Method declarations should be separated by a blank line when members are adjacent")]
    public async Task MethodDeclarations_Should_BeSeparatedByBlankLine_When_MembersAreAdjacent()
    {
        var violations = await ProductionSourceDocumentDiscovery
            .GetAuthorItemsAsync(GetMemberSeparationViolationsAsync);

        Assert.True(
            violations.Length == 0,
            $"Separate methods and constructors from adjacent members with a blank line:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static async Task<string[]> GetClosingBraceViolationsAsync(
        string repositoryRoot,
        Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var source = await document.GetTextAsync();
        var relativePath = Path.GetRelativePath(repositoryRoot, document.FilePath!);

        return
        [
            .. root!.DescendantTokens()
                .Where(token =>
                    token.IsKind(SyntaxKind.CloseBraceToken) &&
                    token.Parent is not InterpolationSyntax &&
                    IsPrecededByBlankLine(token, source))
                .Select(token => source.Lines.GetLineFromPosition(token.SpanStart).LineNumber)
                .Select(line => $"{relativePath}:{line + 1}")
        ];
    }

    private static bool IsPrecededByBlankLine(
        SyntaxToken token,
        SourceText source)
    {
        var line = source.Lines.GetLineFromPosition(token.SpanStart);

        return line.LineNumber > 0 &&
               string.IsNullOrWhiteSpace(source.ToString(TextSpan.FromBounds(line.Start, token.SpanStart))) &&
               string.IsNullOrWhiteSpace(source.Lines[line.LineNumber - 1].ToString());
    }

    private static async Task<string[]> GetMemberSeparationViolationsAsync(
        string repositoryRoot,
        Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var source = await document.GetTextAsync();
        var relativePath = Path.GetRelativePath(repositoryRoot, document.FilePath!);
        var violations = new List<string>();

        foreach (var type in root!.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            for (var index = 1; index < type.Members.Count; index++)
            {
                var previous = type.Members[index - 1];
                var current = type.Members[index];

                if (previous is not BaseMethodDeclarationSyntax &&
                    current is not BaseMethodDeclarationSyntax)
                {
                    continue;
                }

                var previousLine = source.Lines.GetLineFromPosition(previous.Span.End - 1).LineNumber;
                var currentLine = GetLeadingContentLine(current, source);
                var separatorLineCount = Math.Max(0, currentLine - previousLine - 1);
                var hasBlankLine = Enumerable.Range(previousLine + 1, separatorLineCount)
                    .Any(line => string.IsNullOrWhiteSpace(source.Lines[line].ToString()));

                if (!hasBlankLine)
                {
                    violations.Add($"{relativePath}:{currentLine + 1}");
                }
            }
        }

        return [.. violations];
    }

    private static int GetLeadingContentLine(
        MemberDeclarationSyntax member,
        SourceText source)
    {
        var leadingContent = member.GetLeadingTrivia()
            .FirstOrDefault(trivia =>
                !trivia.IsKind(SyntaxKind.WhitespaceTrivia) &&
                !trivia.IsKind(SyntaxKind.EndOfLineTrivia));
        var position = leadingContent.RawKind == 0
            ? member.SpanStart
            : leadingContent.SpanStart;

        return source.Lines.GetLineFromPosition(position).LineNumber;
    }
}
