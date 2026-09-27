using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.Domain.Entities;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class NullForgivingConventionTests
{
    [Fact(DisplayName = "Null-forgiving expressions should avoid suppressed null values when production code is declared")]
    public async Task NullForgivingExpressions_Should_AvoidSuppressedNullValues_When_ProductionCodeIsDeclared()
    {
        var violations = await ProductionSourceDocumentDiscovery.GetItemsAsync(GetDocumentViolationsAsync);

        Assert.True(violations.Length == 0,
            $"Suppressed null values in production code:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Null-forgiving expressions should detect suppressed null values when source expressions vary")]
    [InlineData("public static string Get() => null!;", true)]
    [InlineData("public static string Get() => default!;", true)]
    [InlineData("public static string Get() => default(string)!;", true)]
    [InlineData("public static string Get() => (null)!;", true)]
    [InlineData("public static string Get() => (default(string))!;", true)]
    [InlineData("public static string Get() => ((string?)null)!;", true)]
    [InlineData("public static string Get() { const string? missing = null; return missing!; }", true)]
    [InlineData("public static string Get() { string value = null!; return value; }", true)]
    [InlineData("public static string Get() { string value; value = default!; return value; }", true)]
    [InlineData("public string Value { get; } = null!;", true)]
    [InlineData("public readonly string Value = default!;", true)]
    [InlineData("public static int Get() => int.Parse(null!);", true)]
    [InlineData("public static object Get() => new System.IO.StringReader(default!);", true)]
    [InlineData("public static string Get(string value = null!) => value;", true)]
    [InlineData("public static string[] Get() => [null!];", true)]
    [InlineData("public static string[] Get() => new[] { default(string)! };", true)]
    [InlineData("public static System.Func<string> Get() => () => default!;", true)]
    [InlineData("public static string Get() { string Inner() => null!; return Inner(); }", true)]
    [InlineData("public static System.Collections.Generic.IEnumerable<string> Get() { yield return null!; }", true)]
    [InlineData("public static T Get<T>() => default!;", true)]
    [InlineData("public static T Get<T>() where T : class => default(T)!;", true)]
    [InlineData("public static T Get<T>() where T : notnull => default!;", true)]
    [InlineData("public static int? Get() => default(int?)!;", true)]
    [InlineData("public static string? Get() => null!;", true)]
    [InlineData("public static string? Get() => default!;", true)]
    [InlineData("public static Hero Get() => default!;", true)]
    [InlineData("public static HeroCombatStats Get() => null!;", true)]
    [InlineData("public static string? Get() => default;", false)]
    [InlineData("public static string? Get() => null;", false)]
    [InlineData("public static int Get() => default!;", false)]
    [InlineData("public static T Get<T>() where T : struct => default!;", false)]
    [InlineData("public static object Get() => ((object)default(int))!;", false)]
    [InlineData("public static string Get(string? value) => value!;", false)]
    [InlineData("public static string Get() => string.Empty!;", false)]
    [InlineData("private Sample(int _) : this() { Stats = null!; } public HeroCombatStats Stats { get; private set; }", false)]
    [InlineData("private Sample(int _) : this() { this.Stats = (default!); } public HeroCombatStats Stats { get; private set; }", false)]
    [InlineData("public Sample(int _) : this() { Stats = null!; } public HeroCombatStats Stats { get; private set; }", true)]
    [InlineData("public void Reset() { Stats = null!; } public HeroCombatStats Stats { get; private set; }", true)]
    [InlineData("private Sample(int _) : this() { Name = null!; } public HeroName Name { get; private set; }", true)]
    [InlineData("private Sample(Sample other) : this() { other.Stats = null!; } public HeroCombatStats Stats { get; private set; }", true)]
    [InlineData("private Sample(int _) : this() { void Reset() { Stats = null!; } Reset(); } public HeroCombatStats Stats { get; private set; }", true)]
    [InlineData("public HeroCombatStats Stats { get; private set; } = null!;", true)]
    [InlineData("private Sample(int _) : this() { Stats = null!; } public HeroCombatStats Stats { get; set; }", true)]
    [InlineData("private Sample(int _) : this() { Stats = null!; } public static HeroCombatStats Stats { get; private set; }", true)]
    [InlineData("private Sample(int _) : this() { _stats = null!; } private HeroCombatStats _stats;", true)]
    [InlineData("public DbSet<Hero> Heroes { get; set; } = null!;", true)]
    [InlineData("public sealed class Context : DbContext { public DbSet<Hero> Heroes { get; set; } = null!; }", false)]
    [InlineData("public sealed class Context : DbContext { public DbSet<Hero> Heroes { get; set; } = (default!); }", false)]
    [InlineData("public sealed class Context : DbContext { public string Name { get; set; } = null!; }", true)]
    [InlineData("public sealed class Context : DbContext { public static DbSet<Hero> Heroes { get; set; } = null!; }", true)]
    [InlineData("public sealed class Context : DbContext { public DbSet<Hero> Heroes { get; set; } public void Reset() { Heroes = null!; } }", true)]
    [InlineData("public sealed class Context : DbContext { public DbSet<Hero> Heroes => null!; }", true)]
    [InlineData("public sealed class Context : DbContext { public DbSet<Hero> Heroes = null!; }", true)]
    public void NullForgivingExpressions_Should_DetectSuppressedNullValues_When_SourceExpressionsVary(string member, bool expected)
    {
        var source = $$"""
                       #nullable enable
                       using Microsoft.EntityFrameworkCore;
                       using PANiXiDA.Core.Domain.Entities;
                       using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
                       using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

                       public sealed class Sample : Entity<HeroId>
                       {
                           public Sample() : base(HeroId.New()) { }
                           {{member}}
                       }
                       """;
        var cancellationToken = TestContext.Current.CancellationToken;
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Concat([typeof(IEntity).Assembly.Location, typeof(Hero).Assembly.Location, typeof(DbContext).Assembly.Location])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            assemblyName: "NullForgivingProbe", syntaxTrees: [tree], references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.DoesNotContain(compilation.GetDiagnostics(cancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        var violations = GetViolations(tree.GetRoot(cancellationToken), compilation.GetSemanticModel(tree));

        Assert.Equal(expected, violations.Length > 0);
    }

    private static async Task<string[]> GetDocumentViolationsAsync(string repositoryRoot, Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var model = await document.GetSemanticModelAsync();

        if (root is null || model is null || document.FilePath is null)
        {
            return [];
        }

        var relativePath = Path.GetRelativePath(repositoryRoot, document.FilePath);
        return [.. GetViolations(root, model).Select(expression =>
            $"{relativePath}:{expression.GetLocation().GetLineSpan().StartLinePosition.Line + 1}: " +
            $"'{expression}' suppresses a null value outside EF initialization.")];
    }

    private static PostfixUnaryExpressionSyntax[] GetViolations(SyntaxNode root, SemanticModel model)
    {
        return [.. root.DescendantNodes().OfType<PostfixUnaryExpressionSyntax>()
            .Where(expression => expression.IsKind(SyntaxKind.SuppressNullableWarningExpression) &&
                SuppressesNullValue(expression.Operand, model) && !IsEfInitialization(expression, model))];
    }

    private static bool SuppressesNullValue(ExpressionSyntax expression, SemanticModel model)
    {
        return expression switch
        {
            ParenthesizedExpressionSyntax parentheses => SuppressesNullValue(parentheses.Expression, model),
            CastExpressionSyntax cast => SuppressesNullValue(cast.Expression, model),
            PostfixUnaryExpressionSyntax suppression when suppression.IsKind(SyntaxKind.SuppressNullableWarningExpression) =>
                SuppressesNullValue(suppression.Operand, model),
            DefaultExpressionSyntax => CanBeNull(model.GetTypeInfo(expression).Type),
            LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.DefaultLiteralExpression) =>
                CanBeNull(model.GetTypeInfo(expression).ConvertedType),
            _ => model.GetConstantValue(expression) is { HasValue: true, Value: null }
        };
    }

    private static bool CanBeNull(ITypeSymbol? type)
    {
        return type is not { IsValueType: true } ||
            type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    private static bool IsEfInitialization(ExpressionSyntax expression, SemanticModel model)
    {
        while (expression.Parent is ParenthesizedExpressionSyntax parentheses)
        {
            expression = parentheses;
        }

        if (expression.Parent is EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax declaration } &&
            model.GetDeclaredSymbol(declaration) is { IsStatic: false } property &&
            property.GetMethod?.DeclaredAccessibility == Accessibility.Public &&
            property.SetMethod?.DeclaredAccessibility == Accessibility.Public &&
            declaration.AccessorList?.Accessors.All(accessor => accessor.Body is null && accessor.ExpressionBody is null) == true)
        {
            return IsType(property.Type.OriginalDefinition, model.Compilation, "Microsoft.EntityFrameworkCore.DbSet`1") &&
                InheritsFrom(property.ContainingType, model.Compilation, "Microsoft.EntityFrameworkCore.DbContext");
        }

        return expression.Parent is AssignmentExpressionSyntax assignment &&
            assignment.Right == expression && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
            assignment.Parent is ExpressionStatementSyntax { Parent: BlockSyntax { Parent: ConstructorDeclarationSyntax } } &&
            model.GetEnclosingSymbol(expression.SpanStart) is IMethodSymbol
            {
                MethodKind: MethodKind.Constructor,
                DeclaredAccessibility: Accessibility.Private
            } constructor &&
            model.GetOperation(assignment.Left) is IPropertyReferenceOperation
            {
                Instance: IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance },
                Property: { IsStatic: false, SetMethod.DeclaredAccessibility: Accessibility.Private } target
            } &&
            SymbolEqualityComparer.Default.Equals(target.ContainingType, constructor.ContainingType) &&
            target.ContainingType.AllInterfaces.Any(type => IsType(type, model.Compilation, typeof(IEntity).FullName!)) &&
            InheritsFrom(target.Type, model.Compilation, "PANiXiDA.Core.Domain.ValueObjects.ValueObject") &&
            target.Type.GetMembers().OfType<IPropertySymbol>()
                .Count(property => !property.IsStatic && !property.IsIndexer && !property.IsImplicitlyDeclared) > 1;
    }

    private static bool InheritsFrom(ITypeSymbol type, Compilation compilation, string metadataName)
    {
        for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
        {
            if (IsType(current, compilation, metadataName))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsType(ITypeSymbol type, Compilation compilation, string metadataName)
    {
        return SymbolEqualityComparer.Default.Equals(type, compilation.GetTypeByMetadataName(metadataName));
    }
}
