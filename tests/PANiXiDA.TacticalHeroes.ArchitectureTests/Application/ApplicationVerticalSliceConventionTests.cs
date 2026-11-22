using FluentValidation;

using PANiXiDA.Core.Application.Messaging.EventBus.Handlers;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Querying;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class ApplicationVerticalSliceConventionTests
{
    private const string AbstractionsNamespaceSegment = "Abstractions";
    private const string ApplicationAssemblySuffix = ".Application";
    private const string CommandSuffix = "Command";
    private const string DomainAssemblySuffix = ".Domain";
    private const string HandlerSuffix = "Handler";
    private const string QuerySuffix = "Query";
    private const string SourceDirectoryName = "src";
    private const string ValidatorSuffix = "Validator";

    private static readonly Type[] s_handlerInterfaceDefinitions =
    [
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>)
    ];

    private static readonly Type[] s_requestInterfaceDefinitions =
    [
        typeof(ICommand<>),
        typeof(IQuery<>)
    ];

    [Fact(DisplayName = "Application use cases should reside in feature folders when declared")]
    public void ApplicationUseCases_Should_ResideInFeatureFolders_When_Declared()
    {
        var useCases = GetApplicationUseCases();
        var violations = useCases
            .SelectMany(GetUseCaseLocationViolations)
            .ToArray();

        Assert.NotEmpty(useCases);
        Assert.True(
            violations.Length == 0,
            $"Application use case location violations:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Application support types should reside in feature support folders when declared")]
    public void ApplicationSupportTypes_Should_ResideInFeatureSupportFolders_When_Declared()
    {
        var repositoryRoot = FindRepositoryRoot();
        var applicationTypes = GetApplicationTypes();

        var violations = applicationTypes
            .SelectMany(target => GetSupportTypeLocationViolations(repositoryRoot, target))
            .ToArray();

        Assert.NotEmpty(applicationTypes);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Application read models should reside in a use case or Common ReadModels when declared")]
    public void ApplicationReadModels_Should_ResideInUseCasesOrCommonReadModels_When_Declared()
    {
        var repositoryRoot = FindRepositoryRoot();
        var readModels = GetApplicationTypes()
            .Where(target => typeof(IReadModel).IsAssignableFrom(target.Type))
            .ToArray();
        var violations = readModels
            .SelectMany(target => GetReadModelLocationViolations(repositoryRoot, target))
            .ToArray();

        Assert.NotEmpty(readModels);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Application use case types should have expected role suffixes when declared")]
    public void ApplicationUseCaseTypes_Should_HaveExpectedRoleSuffixes_When_Declared()
    {
        var useCases = GetApplicationUseCases();
        var applicationTypes = GetApplicationTypes()
            .Select(target => target.Type)
            .ToArray();
        var violations = useCases
            .SelectMany(useCase => GetUseCaseRoleSuffixViolations(
                useCase,
                applicationTypes))
            .ToArray();

        Assert.NotEmpty(useCases);
        Assert.True(
            violations.Length == 0,
            $"Application use case role suffix violations:"
                + $"{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Application use case parts should share one feature folder when declared")]
    public void ApplicationUseCaseParts_Should_ShareOneFeatureFolder_When_Declared()
    {
        var repositoryRoot = FindRepositoryRoot();
        var useCases = GetApplicationUseCases();
        var applicationTypes = GetApplicationTypes()
            .Select(target => target.Type)
            .ToArray();
        var violations = useCases
            .SelectMany(useCase => GetUseCaseColocationViolations(
                repositoryRoot,
                useCase,
                applicationTypes))
            .ToArray();

        Assert.NotEmpty(useCases);
        Assert.True(
            violations.Length == 0,
            $"Application use case colocation violations:"
                + $"{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Abstractions namespaces should contain only abstractions when declared")]
    public void AbstractionsNamespaces_Should_ContainOnlyAbstractions_When_Declared()
    {
        var repositoryRoot = FindRepositoryRoot();
        var abstractionTypes = ArchitectureDefinition.s_productionAssemblies
            .Where(assembly =>
                IsLayerAssembly(assembly, DomainAssemblySuffix)
                    || IsLayerAssembly(assembly, ApplicationAssemblySuffix))
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => HasNamespaceSegment(
                type,
                AbstractionsNamespaceSegment))
            .ToArray();
        var violations = abstractionTypes
            .Where(type =>
                !type.IsInterface
                    && !type.IsAbstract
                    && !typeof(Delegate).IsAssignableFrom(type))
            .Select(type =>
                $"{type.FullName} is concrete and must not be declared in "
                    + $"an '{AbstractionsNamespaceSegment}' namespace.")
            .Concat(abstractionTypes
                .Where(type => !File.Exists(GetExpectedSourceFilePath(
                    repositoryRoot,
                    GetModule(type),
                    type)))
                .Select(type =>
                    $"{type.FullName} must have a matching source file in "
                        + $"its '{AbstractionsNamespaceSegment}' directory."))
            .Concat(GetAbstractionSourceFiles(repositoryRoot)
                .Where(sourceFile => !abstractionTypes.Any(type =>
                    string.Equals(
                        GetExpectedSourceFilePath(
                            repositoryRoot,
                            GetModule(type),
                            type),
                        sourceFile,
                        StringComparison.OrdinalIgnoreCase)))
                .Select(sourceFile =>
                    $"'{Path.GetRelativePath(repositoryRoot, sourceFile)}' "
                        + $"must declare only an abstraction in an "
                        + $"'{AbstractionsNamespaceSegment}' namespace."))
            .ToArray();

        Assert.NotEmpty(abstractionTypes);
        Assert.True(
            violations.Length == 0,
            $"Abstractions namespace violations:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<string> GetUseCaseLocationViolations(
        ApplicationUseCase useCase)
    {
        var isQuery = GetRequestSuffix(useCase.RequestType) == QuerySuffix;
        var segments = useCase.RelativeNamespaceSegments;

        if (segments is [_, var operationGroup, _, ..]
            && (isQuery
                ? operationGroup == "Read"
                : operationGroup is "Write" or "Recurring" or "Deferred"))
        {
            yield break;
        }

        var expectedGroups = isQuery ? "Read" : "Write, Recurring, or Deferred";
        yield return $"{useCase.RequestType.FullName} must reside under <Feature>/<OperationGroup>/<UseCase> "
            + $"with {expectedGroups} as its operation group.";
    }

    private static IEnumerable<string> GetSupportTypeLocationViolations(
        string repositoryRoot,
        ApplicationType target)
    {
        var type = target.Type;
        var expectedFolders = GetSupportTypeFolders(type);
        if (expectedFolders.Length == 0)
        {
            yield break;
        }

        var segments = GetRelativeNamespace(type, target.Module.ApplicationAssemblyName).Split('.');
        if (!segments.Skip(1).Take(expectedFolders.Length).SequenceEqual(expectedFolders, StringComparer.Ordinal)
            || !File.Exists(GetExpectedSourceFilePath(repositoryRoot, target.Module, type)))
        {
            yield return $"{type.FullName} must have a matching source file under "
                + $"<Feature>/{string.Join('/', expectedFolders)}.";
        }
    }

    private static IEnumerable<string> GetReadModelLocationViolations(
        string repositoryRoot,
        ApplicationType target)
    {
        var type = target.Type;
        var assemblyName = target.Module.ApplicationAssemblyName;
        string[] segments = type.Namespace == assemblyName
            ? []
            : GetRelativeNamespace(type, assemblyName).Split('.');
        var isCommonModel = segments is [_, "Common", "ReadModels", ..];
        var isUseCaseModel = segments is [_, "Read" or "Write" or "Recurring" or "Deferred", _, ..];

        if ((!isCommonModel && !isUseCaseModel)
            || !File.Exists(GetExpectedSourceFilePath(repositoryRoot, target.Module, type)))
        {
            yield return $"{type.FullName} must have a matching source file in a use case folder "
                + "or under <Feature>/Common/ReadModels.";
        }
    }

    private static string[] GetSupportTypeFolders(Type type)
    {
        if (!type.IsClass)
        {
            return [];
        }

        if (type.GetInterfaces().Any(candidate =>
            candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
        {
            return ["Events"];
        }

        return type.Name switch
        {
            var name when name.EndsWith("Policy", StringComparison.Ordinal) => ["Policies"],
            var name when name.EndsWith("Service", StringComparison.Ordinal) => ["Services"],
            var name when name.EndsWith("Options", StringComparison.Ordinal) => ["Common", "Options"],
            var name when name.EndsWith("Factory", StringComparison.Ordinal) => ["Common", "Factories"],
            _ => []
        };
    }

    private static List<string> GetUseCaseRoleSuffixViolations(
        ApplicationUseCase useCase,
        IReadOnlyCollection<Type> applicationTypes)
    {
        var violations = new List<string>();
        var expectedRequestSuffix = GetRequestSuffix(useCase.RequestType);

        if (!useCase.RequestType.Name.EndsWith(
            expectedRequestSuffix,
            StringComparison.Ordinal))
        {
            violations.Add(
                $"{useCase.RequestType.FullName} must end with "
                    + $"'{expectedRequestSuffix}'.");
        }

        violations.AddRange(applicationTypes
            .Where(type => GetHandlerRequestTypes(type)
                .Contains(useCase.RequestType))
            .Where(type => !type.Name.EndsWith(
                HandlerSuffix,
                StringComparison.Ordinal))
            .Select(type =>
                $"{type.FullName} must end with '{HandlerSuffix}'."));
        violations.AddRange(applicationTypes
            .Where(type => GetValidatedTypes(type)
                .Contains(useCase.RequestType))
            .Where(type => !type.Name.EndsWith(
                ValidatorSuffix,
                StringComparison.Ordinal))
            .Select(type =>
                $"{type.FullName} must end with '{ValidatorSuffix}'."));

        return violations;
    }

    private static List<string> GetUseCaseColocationViolations(
        string repositoryRoot,
        ApplicationUseCase useCase,
        IReadOnlyCollection<Type> applicationTypes)
    {
        var handlers = applicationTypes
            .Where(type => GetHandlerRequestTypes(type)
                .Contains(useCase.RequestType))
            .ToArray();
        var validators = applicationTypes
            .Where(type => GetValidatedTypes(type)
                .Contains(useCase.RequestType))
            .ToArray();
        var violations = new List<string>();

        if (handlers.Length != 1)
        {
            violations.Add(
                $"{useCase.RequestType.FullName} must have exactly one "
                    + $"handler, found: {FormatTypes(handlers)}.");
        }

        if (validators.Length != 1)
        {
            violations.Add(
                $"{useCase.RequestType.FullName} must have exactly one "
                    + $"validator, found: {FormatTypes(validators)}.");
        }

        var requestSuffix = GetRequestSuffix(useCase.RequestType);
        var requestStem =
            useCase.RequestType.Name[..^requestSuffix.Length];
        var expectedHandlerName = requestStem + HandlerSuffix;
        var expectedValidatorName =
            useCase.RequestType.Name + ValidatorSuffix;

        violations.AddRange(handlers
            .Where(handler =>
                !string.Equals(
                    handler.Name,
                    expectedHandlerName,
                    StringComparison.Ordinal)
                    || !string.Equals(
                        handler.Namespace,
                        useCase.RequestType.Namespace,
                        StringComparison.Ordinal))
            .Select(handler =>
                $"{handler.FullName} must be named '{expectedHandlerName}' "
                    + $"and share namespace '{useCase.RequestType.Namespace}' with "
                    + $"{useCase.RequestType.Name}."));
        violations.AddRange(validators
            .Where(validator =>
                !string.Equals(
                    validator.Name,
                    expectedValidatorName,
                    StringComparison.Ordinal)
                    || !string.Equals(
                        validator.Namespace,
                        useCase.RequestType.Namespace,
                        StringComparison.Ordinal))
            .Select(validator =>
                $"{validator.FullName} must be named "
                    + $"'{expectedValidatorName}' and share namespace "
                    + $"'{useCase.RequestType.Namespace}' with "
                    + $"{useCase.RequestType.Name}."));

        var colocatedTypes = handlers
            .Concat(validators)
            .Append(useCase.RequestType)
            .ToArray();
        violations.AddRange(colocatedTypes
            .Where(type => !File.Exists(GetExpectedSourceFilePath(
                repositoryRoot,
                useCase.Module,
                type)))
            .Select(type =>
                $"{type.FullName} must have a matching source file in its "
                    + $"Application feature folder."));

        return violations;
    }

    private static ApplicationUseCase[] GetApplicationUseCases()
    {
        return
        [
            .. ArchitectureDefinition.s_modules
                .SelectMany(module =>
                {
                    var applicationAssembly =
                        ArchitectureDefinition.s_productionAssemblies.Single(
                            assembly => string.Equals(
                                assembly.GetName().Name,
                                module.ApplicationAssemblyName,
                                StringComparison.Ordinal));

                    return applicationAssembly
                        .GetTypes()
                        .Where(IsRequestType)
                        .Select(requestType =>
                        {
                            var relativeNamespaceSegments =
                                GetRelativeNamespace(
                                    requestType,
                                    module.ApplicationAssemblyName)
                                    .Split(
                                        '.',
                                        StringSplitOptions.RemoveEmptyEntries);
                            return new ApplicationUseCase(
                                Module: module,
                                RequestType: requestType,
                                RelativeNamespaceSegments:
                                    relativeNamespaceSegments);
                        });
                })
                .OrderBy(
                    useCase => useCase.RequestType.FullName,
                    StringComparer.Ordinal)
        ];
    }

    private static ApplicationType[] GetApplicationTypes()
    {
        var productionAssemblies = ArchitectureDefinition.s_productionAssemblies
            .ToDictionary(
                assembly => assembly.GetName().Name
                    ?? throw new InvalidOperationException(
                        $"Could not determine assembly name for "
                            + $"'{assembly.FullName}'."),
                StringComparer.Ordinal);

        return
        [
            .. ArchitectureDefinition.s_modules
                .SelectMany(module => productionAssemblies[
                    module.ApplicationAssemblyName]
                    .GetTypes()
                    .Where(type => type.Namespace is not null)
                    .Select(type => new ApplicationType(module, type)))
        ];
    }

    private static Type[] GetHandlerRequestTypes(Type type)
    {
        return
        [
            .. type.GetInterfaces()
                .Where(candidate =>
                    candidate.IsGenericType
                        && s_handlerInterfaceDefinitions.Contains(
                            candidate.GetGenericTypeDefinition()))
                .Select(candidate => candidate.GetGenericArguments()[0])
        ];
    }

    private static Type[] GetValidatedTypes(Type type)
    {
        for (var currentType = type;
             currentType is not null;
             currentType = currentType.BaseType)
        {
            if (currentType.IsGenericType
                && currentType.GetGenericTypeDefinition()
                == typeof(AbstractValidator<>))
            {
                return [currentType.GetGenericArguments()[0]];
            }
        }

        return [];
    }

    private static bool IsRequestType(Type type)
    {
        return type.GetInterfaces().Any(candidate =>
            candidate.IsGenericType
                && s_requestInterfaceDefinitions.Contains(
                    candidate.GetGenericTypeDefinition()));
    }

    private static string GetRequestSuffix(Type requestType)
    {
        return requestType.GetInterfaces().Any(candidate =>
            candidate.IsGenericType
                && candidate.GetGenericTypeDefinition() == typeof(ICommand<>))
            ? CommandSuffix
            : QuerySuffix;
    }

    private static bool HasNamespaceSegment(
        Type type,
        string namespaceSegment)
    {
        return type.Namespace?
            .Split('.')
            .Contains(namespaceSegment, StringComparer.Ordinal) == true;
    }

    private static bool IsLayerAssembly(
        System.Reflection.Assembly assembly,
        string layerSuffix)
    {
        return assembly.GetName().Name?.EndsWith(
            layerSuffix,
            StringComparison.Ordinal) == true;
    }

    private static string GetExpectedSourceFilePath(
        string repositoryRoot,
        ModuleArchitecture module,
        Type type)
    {
        var assemblyName = type.Assembly.GetName().Name
            ?? throw new InvalidOperationException(
                $"Could not determine assembly for '{type.FullName}'.");
        var moduleDirectoryName =
            module.Name[(module.Name.LastIndexOf('.') + 1)..];
        var relativeNamespace = GetRelativeNamespace(
            type,
            assemblyName);

        return Path.Combine(
            repositoryRoot,
            SourceDirectoryName,
            ArchitectureDefinition.ModulesDirectoryName,
            moduleDirectoryName,
            assemblyName,
            relativeNamespace.Replace(
                '.',
                Path.DirectorySeparatorChar),
            $"{type.Name.Split('`')[0]}.cs");
    }

    private static string[] GetAbstractionSourceFiles(
        string repositoryRoot)
    {
        return
        [
            .. ArchitectureDefinition.s_modules
                .SelectMany(module => new[]
                    {
                        module.DomainAssemblyName,
                        module.ApplicationAssemblyName
                    }
                    .SelectMany(assemblyName =>
                    {
                        var moduleDirectoryName =
                            module.Name[(module.Name.LastIndexOf('.') + 1)..];
                        var projectDirectory = Path.Combine(
                            repositoryRoot,
                            SourceDirectoryName,
                            ArchitectureDefinition.ModulesDirectoryName,
                            moduleDirectoryName,
                            assemblyName);

                        return Directory
                            .EnumerateFiles(
                                projectDirectory,
                                "*.cs",
                                SearchOption.AllDirectories)
                            .Where(path =>
                                HasPathSegment(
                                    path,
                                    AbstractionsNamespaceSegment)
                                    && !HasPathSegment(path, "bin")
                                    && !HasPathSegment(path, "obj"));
                    }))
                .Order(StringComparer.OrdinalIgnoreCase)
        ];
    }

    private static ModuleArchitecture GetModule(Type type)
    {
        var assemblyName = type.Assembly.GetName().Name
            ?? throw new InvalidOperationException(
                $"Could not determine assembly for '{type.FullName}'.");

        return ArchitectureDefinition.s_modules.Single(module =>
            string.Equals(
                module.DomainAssemblyName,
                assemblyName,
                StringComparison.Ordinal)
                || string.Equals(
                    module.ApplicationAssemblyName,
                    assemblyName,
                    StringComparison.Ordinal));
    }

    private static bool HasPathSegment(
        string path,
        string segment)
    {
        return path
            .Split(
                [
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                ],
                StringSplitOptions.RemoveEmptyEntries)
            .Contains(segment, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetRelativeNamespace(
        Type type,
        string assemblyName)
    {
        var typeNamespace = type.Namespace
            ?? throw new InvalidOperationException(
                $"Type '{type.FullName}' does not have a namespace.");
        var namespacePrefix = assemblyName + ".";

        if (!typeNamespace.StartsWith(
            namespacePrefix,
            StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Type '{type.FullName}' namespace must start with "
                    + $"'{namespacePrefix}'.");
        }

        return typeNamespace[namespacePrefix.Length..];
    }

    private static string FormatTypes(
        Type[] types)
    {
        return types.Length == 0
            ? "<none>"
            : string.Join(
                ", ",
                types
                    .Select(type => type.FullName)
                    .Order(StringComparer.Ordinal));
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
            $"Could not find repository root containing "
                + $"the '{SourceDirectoryName}' directory.");
    }

    private sealed record ApplicationType(
        ModuleArchitecture Module,
        Type Type);

    private sealed record ApplicationUseCase(
        ModuleArchitecture Module,
        Type RequestType,
        string[] RelativeNamespaceSegments);
}
