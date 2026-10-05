using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class MemberOrderConventionTests
{
    [Fact(DisplayName = "Type members should follow the agreed order when declared")]
    public async Task TypeMembers_Should_FollowAgreedOrder_When_Declared()
    {
        var documents = await ProductionSourceDocumentDiscovery
            .GetAuthorItemsAsync(MemberOrderConvention.GetDocumentViolationsAsync);
        var violations = documents.SelectMany(document => document).Distinct().ToArray();

        Assert.NotEmpty(documents);
        Assert.True(
            violations.Length == 0,
            $"Member ordering violations: {violations.Length} total.{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Member order should accept declarations when order is valid")]
    [InlineData("class C { private const int Value = 1; public int Field; private C() {} public int Property { get; } public void M() {} private class Nested {} }")]
    [InlineData("class C { public void A() {} internal void B() {} protected internal void D() {} protected void E() {} private protected void F() {} private void G() {} }")]
    [InlineData("class C { public void A() {} private static void B() {} private void D() {} }")]
    [InlineData("class C { public static readonly int A; public static int B; public readonly int D; public int E; private static readonly int F; }")]
    [InlineData("class C { static C() {} public C() {} ~C() {} public event System.Action E { add {} remove {} } public int P => 0; public int this[int i] => i; public void M() {} public static C operator +(C a, C b) => a; private delegate void D(); }")]
    [InlineData("interface I { void A(); private static void B() {} }")]
    [InlineData("class C : System.IDisposable { public void A() {} void System.IDisposable.Dispose() {} private void B() {} }")]
    [InlineData("interface I<T> where T : I<T> { static abstract T operator +(T a, T b); } class C : I<C> { static C I<C>.operator +(C a, C b) => a; public static C operator -(C a) => a; }")]
    [InlineData("interface I<T> where T : I<T> { static abstract explicit operator int(T v); } class C : I<C> { static explicit I<C>.operator int(C v) => 0; public static explicit operator long(C v) => 0; }")]
    [InlineData("record C(int Value) { public int A => Value; private int B => Value; private record Nested { public void A() {} private void B() {} } }")]
    [InlineData("partial class C { private void A() {} } partial class C { public void B() {} }")]
    [InlineData("struct S { public int B() => 0; public readonly int A() => 0; }")]
    [InlineData("public static class E { public static void A() {} extension(string value) { public int P => value.Length; public static string B() => string.Empty; public int C() => value.Length; private int D() => value.Length; } }")]
    [InlineData("public static class E { public static void A() {} extension<T>(T value) where T : class { public bool HasValue => value is not null; public T GetValue() => value; } }")]
    public void MemberOrder_Should_AcceptDeclarations_When_OrderIsValid(string source)
    {
        var root = CSharpSyntaxTree.ParseText(
                source,
                cancellationToken: TestContext.Current.CancellationToken)
            .GetRoot(TestContext.Current.CancellationToken);

        var violations = MemberOrderConvention.GetViolations(root, "Sample.cs");

        Assert.Empty(root.GetDiagnostics());
        Assert.Empty(violations);
    }

    [Theory(DisplayName = "Member order should reject declarations when order is invalid")]
    [InlineData("class C { public int Field; private const int Value = 1; }", "member kind")]
    [InlineData("class C { public void M() {} private int P => 0; }", "member kind")]
    [InlineData("class C { private void A() {} public void B() {} }", "accessibility")]
    [InlineData("class C { protected void A() {} protected internal void B() {} }", "accessibility")]
    [InlineData("class C { private protected void A() {} protected void B() {} }", "accessibility")]
    [InlineData("class C { public void A() {} public static void B() {} }", "static")]
    [InlineData("class C { private int A; private readonly int B; }", "readonly")]
    [InlineData("class C { private static int A; private static readonly int B; }", "readonly")]
    [InlineData("class C { void A() {} public void B() {} }", "accessibility")]
    [InlineData("interface I { private void A() {} void B(); }", "accessibility")]
    [InlineData("class C { private class Nested { private void A() {} public void B() {} } }", "accessibility")]
    [InlineData("public static class E { extension(string value) { public int Length() => value.Length; } public static void A() {} }", "member kind")]
    [InlineData("public static class E { extension(string value) { public int Length() => value.Length; public bool IsEmpty => value.Length == 0; } }", "member kind")]
    [InlineData("public static class E { extension(string value) { private int A() => value.Length; public int B() => value.Length; } }", "accessibility")]
    [InlineData("public static class E { extension(string value) { public int A() => value.Length; public static string B() => string.Empty; } }", "static")]
    public void MemberOrder_Should_RejectDeclarations_When_OrderIsInvalid(
        string source,
        string reason)
    {
        var root = CSharpSyntaxTree.ParseText(
                source,
                cancellationToken: TestContext.Current.CancellationToken)
            .GetRoot(TestContext.Current.CancellationToken);

        var violations = MemberOrderConvention.GetViolations(root, "Sample.cs");

        Assert.Empty(root.GetDiagnostics());
        Assert.Contains(reason, Assert.Single(violations), StringComparison.Ordinal);
        Assert.StartsWith("Sample.cs:1:", violations[0], StringComparison.Ordinal);
    }
}

internal static class MemberOrderConvention
{
    internal static async Task<string[][]> GetDocumentViolationsAsync(
        string repositoryRoot,
        Document document)
    {
        var root = await document.GetSyntaxRootAsync();

        if (root is null || document.FilePath is null)
        {
            return [];
        }

        return [GetViolations(root, Path.GetRelativePath(repositoryRoot, document.FilePath))];
    }

    internal static string[] GetViolations(SyntaxNode root, string relativePath)
    {
        var violations = new List<string>();

        foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            for (var index = 1; index < type.Members.Count; index++)
            {
                var previous = type.Members[index - 1];
                var current = type.Members[index];
                var previousOrder = GetOrder(previous, type);
                var currentOrder = GetOrder(current, type);

                if (previousOrder.CompareTo(currentOrder) <= 0)
                {
                    continue;
                }

                var reason = previousOrder.Kind != currentOrder.Kind ? "member kind"
                    : previousOrder.Accessibility != currentOrder.Accessibility ? "accessibility"
                    : previousOrder.Static != currentOrder.Static ? "static"
                    : "readonly";
                var line = current.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                var previousLine = previous.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                violations.Add(
                    $"{relativePath}:{line}: '{type.Identifier.ValueText}': " +
                    $"{current.Kind()} must precede {previous.Kind()} at line {previousLine} " +
                    $"according to {reason} order.");
            }
        }

        return [.. violations];
    }

    private static (int Kind, int Accessibility, int Static, int Readonly) GetOrder(
        MemberDeclarationSyntax member,
        TypeDeclarationSyntax containingType)
    {
        return (
            GetKindOrder(member),
            GetAccessibilityOrder(member, containingType),
            member.Modifiers.Any(SyntaxKind.StaticKeyword) ? 0 : 1,
            member is FieldDeclarationSyntax && member.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) ? 0 : 1);
    }

    private static int GetKindOrder(MemberDeclarationSyntax member)
    {
        return member switch
        {
            FieldDeclarationSyntax field when field.Modifiers.Any(SyntaxKind.ConstKeyword) => 0,
            FieldDeclarationSyntax => 1,
            ConstructorDeclarationSyntax => 2,
            DestructorDeclarationSyntax => 3,
            EventDeclarationSyntax or EventFieldDeclarationSyntax => 4,
            PropertyDeclarationSyntax => 5,
            IndexerDeclarationSyntax => 6,
            MethodDeclarationSyntax => 7,
            OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax => 8,
            BaseTypeDeclarationSyntax or DelegateDeclarationSyntax => 9,
            _ => throw new InvalidOperationException($"Unsupported member kind: {member.Kind()}.")
        };
    }

    private static int GetAccessibilityOrder(
        MemberDeclarationSyntax member,
        TypeDeclarationSyntax containingType)
    {
        if (member is ConstructorDeclarationSyntax && member.Modifiers.Any(SyntaxKind.StaticKeyword) ||
            member is MethodDeclarationSyntax { ExplicitInterfaceSpecifier: not null } or
                PropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null } or
                IndexerDeclarationSyntax { ExplicitInterfaceSpecifier: not null } or
                EventDeclarationSyntax { ExplicitInterfaceSpecifier: not null } or
                OperatorDeclarationSyntax { ExplicitInterfaceSpecifier: not null } or
                ConversionOperatorDeclarationSyntax { ExplicitInterfaceSpecifier: not null })
        {
            return 0;
        }

        var modifiers = member.Modifiers;

        if (modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return 0;
        }

        if (modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            return modifiers.Any(SyntaxKind.InternalKeyword) ? 2
                : modifiers.Any(SyntaxKind.PrivateKeyword) ? 4
                : 3;
        }

        if (modifiers.Any(SyntaxKind.InternalKeyword))
        {
            return 1;
        }

        return !modifiers.Any(SyntaxKind.PrivateKeyword) && containingType is InterfaceDeclarationSyntax
            ? 0
            : 5;
    }
}
