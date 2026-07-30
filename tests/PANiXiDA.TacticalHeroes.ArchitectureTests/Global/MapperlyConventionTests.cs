using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class MapperlyConventionTests
{
    private const string InfrastructureSuffix = ".Infrastructure";
    private const string PresentationSuffix = ".Presentation";

    [Fact(DisplayName = "Mapperly mappers should reside only in Infrastructure or Presentation when declared")]
    public void MapperlyMappers_Should_ResideOnlyInInfrastructureOrPresentation_When_Declared()
    {
        var mappers = MapperlySourceDiscovery.GetMappers();
        var violations = mappers
            .Where(mapper =>
                !mapper.ProjectName.EndsWith(
                    InfrastructureSuffix,
                    StringComparison.Ordinal) &&
                !mapper.ProjectName.EndsWith(
                    PresentationSuffix,
                    StringComparison.Ordinal))
            .Select(mapper =>
                $"{mapper.RelativePath}: Mapperly mapper " +
                $"'{mapper.FullName}' must reside in an Infrastructure or " +
                $"Presentation project, found '{mapper.ProjectName}'.")
            .ToArray();

        Assert.NotEmpty(mappers);
        Assert.True(
            violations.Length == 0,
            $"Mapperly mapper layer violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Mapperly mappers should disable automatic user mappings when declared")]
    public void MapperlyMappers_Should_DisableAutomaticUserMappings_When_Declared()
    {
        var mappers = MapperlySourceDiscovery.GetMappers();
        var violations = mappers
            .Where(mapper => !mapper.AutomaticUserMappingsDisabled)
            .Select(mapper =>
                $"{mapper.RelativePath}: Mapperly mapper " +
                $"'{mapper.FullName}' must set AutoUserMappings to false " +
                $"directly or through MapperDefaults.")
            .ToArray();

        Assert.NotEmpty(mappers);
        Assert.True(
            violations.Length == 0,
            $"Mapperly automatic user mapping violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Mapperly mapper methods should be partial or ignored helpers when declared")]
    public void MapperlyMapperMethods_Should_BePartialOrIgnoredHelpers_When_Declared()
    {
        var mapperMethods = MapperlySourceDiscovery
            .GetMappers()
            .SelectMany(mapper => mapper.Declarations
                .SelectMany(declaration => declaration.Declaration.Members
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method =>
                        !MapperlySourceDiscovery.IsPrivate(method))
                    .Select(method => new
                    {
                        Mapper = mapper,
                        Declaration = declaration,
                        Method = method
                    })))
            .ToArray();
        var violations = mapperMethods
            .Where(mapperMethod =>
                !MapperlySourceDiscovery.IsGeneratedMapping(
                    mapperMethod.Method) &&
                !MapperlySourceDiscovery.IsIgnoredHelper(
                    mapperMethod.Method))
            .Select(mapperMethod =>
                $"{mapperMethod.Declaration.RelativePath}: mapper method " +
                $"'{mapperMethod.Mapper.FullName}." +
                $"{mapperMethod.Method.Identifier.ValueText}' must be a " +
                $"Mapperly partial mapping or an explicit " +
                $"[MapperIgnore] helper.")
            .ToArray();

        Assert.NotEmpty(mapperMethods);
        Assert.True(
            violations.Length == 0,
            $"Mapperly mapper method role violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Mapperly generated mappings should be partial definitions when declared")]
    public void MapperlyGeneratedMappings_Should_BePartialDefinitions_When_Declared()
    {
        var generatedMappings = MapperlySourceDiscovery
            .GetMappers()
            .SelectMany(mapper => mapper.Declarations
                .SelectMany(declaration => declaration.Declaration.Members
                    .OfType<MethodDeclarationSyntax>()
                    .Where(MapperlySourceDiscovery.IsGeneratedMapping)
                    .Select(method => new
                    {
                        Mapper = mapper,
                        Declaration = declaration,
                        Method = method
                    })))
            .ToArray();
        var violations = generatedMappings
            .Where(mapping =>
                MapperlySourceDiscovery.HasImplementation(mapping.Method))
            .Select(mapping =>
                $"{mapping.Declaration.RelativePath}: Mapperly generated " +
                $"mapping '{mapping.Mapper.FullName}." +
                $"{mapping.Method.Identifier.ValueText}' must be a partial " +
                $"method definition without a body.")
            .ToArray();

        Assert.NotEmpty(generatedMappings);
        Assert.True(
            violations.Length == 0,
            $"Mapperly generated mapping definition violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Mapperly user mappings should be private when declared")]
    public void MapperlyUserMappings_Should_BePrivate_When_Declared()
    {
        var userMappings = MapperlySourceDiscovery
            .GetMappers()
            .SelectMany(mapper => mapper.Declarations
                .SelectMany(declaration => declaration.Declaration.Members
                    .OfType<MethodDeclarationSyntax>()
                    .Where(MapperlySourceDiscovery.IsUserMapping)
                    .Select(method => new
                    {
                        Mapper = mapper,
                        Declaration = declaration,
                        Method = method
                    })))
            .ToArray();
        var violations = userMappings
            .Where(userMapping =>
                !MapperlySourceDiscovery.IsPrivate(userMapping.Method))
            .Select(userMapping =>
                $"{userMapping.Declaration.RelativePath}: manual Mapperly " +
                $"user mapping '{userMapping.Mapper.FullName}." +
                $"{userMapping.Method.Identifier.ValueText}' must be private.")
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Mapperly user mapping visibility violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }
}

internal static class MapperlySourceDiscovery
{
    private const string SourceDirectoryName = "src";

    private static readonly string[] ExcludedDirectoryNames =
    [
        "bin",
        "obj",
        "Generated"
    ];

    internal static MapperlyMapperSource[] GetMappers()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceRoot = Path.Combine(
            repositoryRoot,
            SourceDirectoryName);

        return
        [
            .. Directory
                .EnumerateFiles(
                    sourceRoot,
                    "*.csproj",
                    SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)
                .SelectMany(projectFile =>
                    GetProjectMappers(
                        repositoryRoot,
                        projectFile))
                .OrderBy(
                    mapper => mapper.FullName,
                    StringComparer.Ordinal)
        ];
    }

    internal static bool IsUserMapping(MethodDeclarationSyntax method)
    {
        return GetAttribute(
            method.AttributeLists,
            "UserMapping") is not null;
    }

    internal static bool IsGeneratedMapping(MethodDeclarationSyntax method)
    {
        return method.Modifiers.Any(modifier =>
            modifier.IsKind(SyntaxKind.PartialKeyword));
    }

    internal static bool IsIgnoredHelper(MethodDeclarationSyntax method)
    {
        return GetAttribute(
            method.AttributeLists,
            "MapperIgnore") is not null;
    }

    internal static bool HasImplementation(MethodDeclarationSyntax method)
    {
        return method.Body is not null ||
               method.ExpressionBody is not null;
    }

    internal static bool IsPrivate(MethodDeclarationSyntax method)
    {
        return !method.Modifiers.Any(modifier =>
            modifier.IsKind(SyntaxKind.PublicKeyword) ||
            modifier.IsKind(SyntaxKind.InternalKeyword) ||
            modifier.IsKind(SyntaxKind.ProtectedKeyword));
    }

    private static IEnumerable<MapperlyMapperSource> GetProjectMappers(
        string repositoryRoot,
        string projectFile)
    {
        var projectDirectory = Path.GetDirectoryName(projectFile)
            ?? throw new InvalidOperationException(
                $"Project '{projectFile}' does not have a directory.");
        var documents = Directory
            .EnumerateFiles(
                projectDirectory,
                "*.cs",
                SearchOption.AllDirectories)
            .Where(IsSourceFile)
            .Order(StringComparer.Ordinal)
            .Select(sourceFile => new MapperlySourceDocument(
                RelativePath: Path.GetRelativePath(
                    repositoryRoot,
                    sourceFile),
                Root: CSharpSyntaxTree
                    .ParseText(File.ReadAllText(sourceFile))
                    .GetCompilationUnitRoot()))
            .ToArray();
        var declarations = documents
            .SelectMany(document => document.Root
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Select(declaration => new MapperlyTypeDeclaration(
                    document.RelativePath,
                    declaration)))
            .ToArray();
        var mapperNames = declarations
            .Where(declaration =>
                IsMapper(declaration.Declaration))
            .Select(declaration =>
                GetFullTypeName(declaration.Declaration))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var assemblySetting = GetAssemblyAutomaticUserMappingsSetting(
            documents);

        return mapperNames.Select(mapperName =>
        {
            var mapperDeclarations = declarations
                .Where(declaration => string.Equals(
                    GetFullTypeName(declaration.Declaration),
                    mapperName,
                    StringComparison.Ordinal))
                .ToArray();
            var mapperSetting = mapperDeclarations
                .Select(declaration =>
                    GetAutomaticUserMappingsSetting(
                        GetAttribute(
                            declaration.Declaration.AttributeLists,
                            "Mapper")))
                .OfType<bool>()
                .ToArray();
            var effectiveSetting = mapperSetting.Length == 0
                ? assemblySetting
                : mapperSetting.Contains(true);

            return new MapperlyMapperSource(
                ProjectName: Path.GetFileNameWithoutExtension(projectFile),
                FullName: mapperName,
                RelativePath: mapperDeclarations[0].RelativePath,
                AutomaticUserMappingsDisabled:
                    effectiveSetting == false,
                Declarations: mapperDeclarations);
        });
    }

    private static bool? GetAssemblyAutomaticUserMappingsSetting(
        IEnumerable<MapperlySourceDocument> documents)
    {
        var settings = documents
            .SelectMany(document => document.Root.AttributeLists)
            .Where(attributeList =>
                string.Equals(
                    attributeList.Target?.Identifier.ValueText,
                    "assembly",
                    StringComparison.Ordinal))
            .SelectMany(attributeList =>
                attributeList.Attributes)
            .Where(attribute =>
                IsAttribute(attribute, "MapperDefaults"))
            .Select(GetAutomaticUserMappingsSetting)
            .OfType<bool>()
            .Distinct()
            .ToArray();

        return settings.Length == 1
            ? settings[0]
            : null;
    }

    private static bool? GetAutomaticUserMappingsSetting(
        AttributeSyntax? attribute)
    {
        var argument = attribute?
            .ArgumentList?
            .Arguments
            .SingleOrDefault(argument =>
                string.Equals(
                    argument.NameEquals?.Name.Identifier.ValueText,
                    "AutoUserMappings",
                    StringComparison.Ordinal));

        return argument?.Expression.Kind() switch
        {
            SyntaxKind.TrueLiteralExpression => true,
            SyntaxKind.FalseLiteralExpression => false,
            _ => null
        };
    }

    private static bool IsMapper(
        TypeDeclarationSyntax declaration)
    {
        return GetAttribute(
            declaration.AttributeLists,
            "Mapper") is not null;
    }

    private static AttributeSyntax? GetAttribute(
        SyntaxList<AttributeListSyntax> attributeLists,
        string expectedName)
    {
        return attributeLists
            .SelectMany(attributeList =>
                attributeList.Attributes)
            .FirstOrDefault(attribute =>
                IsAttribute(attribute, expectedName));
    }

    private static bool IsAttribute(
        AttributeSyntax attribute,
        string expectedName)
    {
        var attributeName = attribute.Name switch
        {
            IdentifierNameSyntax identifier =>
                identifier.Identifier.ValueText,
            QualifiedNameSyntax qualifiedName =>
                qualifiedName.Right.Identifier.ValueText,
            AliasQualifiedNameSyntax aliasQualifiedName =>
                aliasQualifiedName.Name.Identifier.ValueText,
            _ => attribute.Name.ToString().Split('.').Last()
        };

        return string.Equals(
                   attributeName,
                   expectedName,
                   StringComparison.Ordinal) ||
               string.Equals(
                   attributeName,
                   expectedName + "Attribute",
                   StringComparison.Ordinal);
    }

    private static string GetFullTypeName(
        TypeDeclarationSyntax declaration)
    {
        var containingNamespace = string.Join(
            '.',
            declaration
                .Ancestors()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .Reverse()
                .Select(namespaceDeclaration =>
                    namespaceDeclaration.Name.ToString()));
        var containingTypes = declaration
            .Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .Reverse()
            .Select(type => type.Identifier.ValueText)
            .Append(declaration.Identifier.ValueText);
        var typeName = string.Join('+', containingTypes);

        return string.IsNullOrWhiteSpace(containingNamespace)
            ? typeName
            : $"{containingNamespace}.{typeName}";
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
            if (Directory.Exists(Path.Combine(
                    directory.FullName,
                    SourceDirectoryName)))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find repository root containing " +
            $"the '{SourceDirectoryName}' directory.");
    }
}

internal sealed record MapperlyMapperSource(
    string ProjectName,
    string FullName,
    string RelativePath,
    bool AutomaticUserMappingsDisabled,
    IReadOnlyCollection<MapperlyTypeDeclaration> Declarations);

internal sealed record MapperlyTypeDeclaration(
    string RelativePath,
    TypeDeclarationSyntax Declaration);

internal sealed record MapperlySourceDocument(
    string RelativePath,
    CompilationUnitSyntax Root);
