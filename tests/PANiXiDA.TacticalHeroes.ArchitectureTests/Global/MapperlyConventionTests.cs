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

    [Fact(DisplayName = "Mapper types should use Mapperly when declared")]
    public void MapperTypes_Should_UseMapperly_When_Declared()
    {
        var mapperTypes = MapperlySourceDiscovery.GetMapperTypes();
        var violations = mapperTypes
            .Where(mapper => !mapper.UsesMapperly)
            .Select(mapper =>
                $"{mapper.RelativePath}: mapper '{mapper.FullName}' must " +
                $"be declared with the Mapperly [Mapper] attribute.")
            .ToArray();

        Assert.NotEmpty(mapperTypes);
        Assert.True(
            violations.Length == 0,
            $"Manual mapper type violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Mapperly mapper methods should be partial or explicitly ignored when declared")]
    public void MapperlyMapperMethods_Should_BePartialOrExplicitlyIgnored_When_Declared()
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
                !MapperlySourceDiscovery.IsExplicitlyIgnored(
                    mapperMethod.Method))
            .Select(mapperMethod =>
                $"{mapperMethod.Declaration.RelativePath}: mapper method " +
                $"'{mapperMethod.Mapper.FullName}." +
                $"{mapperMethod.Method.Identifier.ValueText}' must be a " +
                $"Mapperly partial mapping or an explicit " +
                $"[MapperIgnore] exception.")
            .ToArray();

        Assert.NotEmpty(mapperMethods);
        Assert.True(
            violations.Length == 0,
            $"Mapperly mapper method role violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Mapperly mappers should have generated mappings when declared")]
    public void MapperlyMappers_Should_HaveGeneratedMappings_When_Declared()
    {
        var mappers = MapperlySourceDiscovery.GetMappers();
        var violations = mappers
            .Where(mapper => !mapper.Declarations
                .SelectMany(declaration =>
                    declaration.Declaration.Members
                        .OfType<MethodDeclarationSyntax>())
                .Any(MapperlySourceDiscovery.IsGeneratedMapping))
            .Select(mapper =>
                $"{mapper.RelativePath}: Mapperly mapper " +
                $"'{mapper.FullName}' must declare at least one partial " +
                $"mapping method.")
            .ToArray();

        Assert.NotEmpty(mappers);
        Assert.True(
            violations.Length == 0,
            $"Mapperly generated mapping presence violations:" +
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

    internal static MapperTypeSource[] GetMapperTypes()
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
                    GetProjectMapperTypes(
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

    internal static bool HasImplementation(MethodDeclarationSyntax method)
    {
        return method.Body is not null ||
               method.ExpressionBody is not null;
    }

    internal static bool IsExplicitlyIgnored(MethodDeclarationSyntax method)
    {
        return GetAttribute(
            method.AttributeLists,
            "MapperIgnore") is not null;
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
        var documents = GetProjectDocuments(
            repositoryRoot,
            projectFile);
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

        return mapperNames.Select(mapperName =>
        {
            var mapperDeclarations = declarations
                .Where(declaration => string.Equals(
                    GetFullTypeName(declaration.Declaration),
                    mapperName,
                    StringComparison.Ordinal))
                .ToArray();

            return new MapperlyMapperSource(
                ProjectName: Path.GetFileNameWithoutExtension(projectFile),
                FullName: mapperName,
                RelativePath: mapperDeclarations[0].RelativePath,
                Declarations: mapperDeclarations);
        });
    }

    private static IEnumerable<MapperTypeSource> GetProjectMapperTypes(
        string repositoryRoot,
        string projectFile)
    {
        var mapperTypes = GetProjectDocuments(
                repositoryRoot,
                projectFile)
            .SelectMany(document => document.Root
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Where(declaration => declaration.Identifier.ValueText
                    .EndsWith(
                        "Mapper",
                        StringComparison.Ordinal))
                .Select(declaration => new MapperTypeSource(
                    ProjectName:
                        Path.GetFileNameWithoutExtension(projectFile),
                    FullName: GetFullTypeName(declaration),
                    RelativePath: document.RelativePath,
                    UsesMapperly: IsMapper(declaration))))
            .ToArray();

        return mapperTypes
            .GroupBy(
                mapper => mapper.FullName,
                StringComparer.Ordinal)
            .Select(group => new MapperTypeSource(
                ProjectName: group.First().ProjectName,
                FullName: group.Key,
                RelativePath: group.First().RelativePath,
                UsesMapperly: group.Any(mapper =>
                    mapper.UsesMapperly)))
            .ToArray();
    }

    private static MapperlySourceDocument[] GetProjectDocuments(
        string repositoryRoot,
        string projectFile)
    {
        var projectDirectory = Path.GetDirectoryName(projectFile)
            ?? throw new InvalidOperationException(
                $"Project '{projectFile}' does not have a directory.");

        return
        [
            .. Directory
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
        ];
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
    IReadOnlyCollection<MapperlyTypeDeclaration> Declarations);

internal sealed record MapperTypeSource(
    string ProjectName,
    string FullName,
    string RelativePath,
    bool UsesMapperly);

internal sealed record MapperlyTypeDeclaration(
    string RelativePath,
    TypeDeclarationSyntax Declaration);

internal sealed record MapperlySourceDocument(
    string RelativePath,
    CompilationUnitSyntax Root);
