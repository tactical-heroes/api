using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Presentation;

public sealed class EndpointBehaviorConventionTests
{
    [Fact(DisplayName = "CreatedAtRoute calls should use endpoint names when declared")]
    public void CreatedAtRouteCalls_Should_UseEndpointNames_When_Declared()
    {
        var createdAtRouteCalls = GetEndpointInvocations("CreatedAtRoute");
        var violations = createdAtRouteCalls
            .Where(call => !UsesEndpointRouteName(call.Invocation))
            .Select(call =>
                $"{call.Endpoint.FullName} must pass routeName as " +
                $"new <Target>Endpoint().Name at " +
                $"'{Path.GetFileName(call.SourceFile)}:" +
                $"{GetLineNumber(call.Invocation)}'.")
            .ToArray();

        Assert.NotEmpty(createdAtRouteCalls);
        Assert.True(
            violations.Length == 0,
            $"CreatedAtRoute route name violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Presentation application references should exist only in mappers when declared")]
    public async Task PresentationApplicationReferences_Should_ExistOnlyInMappers_When_Declared()
    {
        var analysis = await PresentationBoundarySourceDiscovery
            .GetAnalysisAsync();
        var violations = analysis.ApplicationReferences
            .Select(reference =>
                $"{reference.RelativePath}:{reference.LineNumber}: " +
                $"'{reference.Expression}' references " +
                $"{reference.ApplicationAssemblyName} outside a Mapper.")
            .ToArray();

        Assert.NotEmpty(analysis.PresentationDocuments);
        Assert.True(
            violations.Length == 0,
            $"Presentation Application reference violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Mediator messages should be created by slice mappers when endpoint sends a message")]
    public async Task MediatorMessages_Should_BeCreatedBySliceMappers_When_EndpointSendsAMessage()
    {
        var analysis = await PresentationBoundarySourceDiscovery
            .GetAnalysisAsync();
        var violations = analysis.MediatorCalls
            .Where(call => !call.UsesPresentationMapper)
            .Select(call =>
                $"{call.RelativePath}:{call.LineNumber}: " +
                $"{call.EndpointTypeName}.{call.MethodName} must receive a " +
                $"command or query created directly by a Presentation " +
                $"Mapper.")
            .ToArray();

        Assert.NotEmpty(analysis.MediatorCalls);
        Assert.True(
            violations.Length == 0,
            $"Endpoint mediator usage violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static EndpointInvocation[] GetEndpointInvocations(
        string methodName)
    {
        return
        [
            .. PresentationArchitectureConvention
                .GetEndpoints()
                .SelectMany(endpoint =>
                    PresentationArchitectureConvention
                        .GetSourceSyntax(endpoint)
                        .SelectMany(source => source.Root
                            .DescendantNodes()
                            .OfType<InvocationExpressionSyntax>()
                            .Where(invocation => string.Equals(
                                GetInvocationName(invocation),
                                methodName,
                                StringComparison.Ordinal))
                            .Select(invocation => new EndpointInvocation(
                                Endpoint: endpoint,
                                SourceFile: source.SourceFile,
                                Invocation: invocation))))
        ];
    }

    private static bool UsesEndpointRouteName(
        InvocationExpressionSyntax invocation)
    {
        var routeNameArgument = invocation.ArgumentList.Arguments
            .SingleOrDefault(argument => string.Equals(
                argument.NameColon?.Name.Identifier.ValueText,
                "routeName",
                StringComparison.Ordinal));

        return routeNameArgument?.Expression is
            MemberAccessExpressionSyntax
        {
            Expression: ObjectCreationExpressionSyntax objectCreation,
            Name.Identifier.ValueText: "Name"
        } &&
            objectCreation.Type.ToString().EndsWith(
                "Endpoint",
                StringComparison.Ordinal);
    }

    private static string? GetInvocationName(
        InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess =>
                memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier =>
                identifier.Identifier.ValueText,
            GenericNameSyntax genericName =>
                genericName.Identifier.ValueText,
            _ => null
        };
    }

    private static int GetLineNumber(
        Microsoft.CodeAnalysis.SyntaxNode node)
    {
        return node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
    }

    private sealed record EndpointInvocation(
        Type Endpoint,
        string SourceFile,
        InvocationExpressionSyntax Invocation);
}

internal static class PresentationBoundarySourceDiscovery
{
    private const string ApplicationAssemblySuffix = ".Application";
    private const string EndpointInterfaceNamespace =
        "PANiXiDA.Core.Presentation.Http.Endpoints";
    private const string MapperSuffix = "Mapper";
    private const string MediatorTypeName =
        "PANiXiDA.Core.Application.Messaging.Mediator.IMediator";
    private const string PresentationAssemblySuffix = ".Presentation";

    private static readonly string[] MediatorMethodNames =
    [
        "QueryAsync",
        "SendAsync"
    ];

    private static readonly Lazy<Task<PresentationBoundaryAnalysis>> Analysis =
        new(CreateAnalysisAsync);

    internal static Task<PresentationBoundaryAnalysis> GetAnalysisAsync()
    {
        return Analysis.Value;
    }

    private static async Task<PresentationBoundaryAnalysis>
        CreateAnalysisAsync()
    {
        var documents = await ProductionSourceDocumentDiscovery
            .GetItemsAsync(GetDocumentAnalysisAsync);

        return new PresentationBoundaryAnalysis(
            PresentationDocuments:
            [
                .. documents
                    .Select(document => document.RelativePath)
                    .Order(StringComparer.Ordinal)
            ],
            ApplicationReferences:
            [
                .. documents
                    .SelectMany(document => document.ApplicationReferences)
                    .Distinct()
                    .OrderBy(
                        reference => reference.RelativePath,
                        StringComparer.Ordinal)
                    .ThenBy(reference => reference.Position)
            ],
            MediatorCalls:
            [
                .. documents
                    .SelectMany(document => document.MediatorCalls)
                    .OrderBy(
                        call => call.RelativePath,
                        StringComparer.Ordinal)
                    .ThenBy(call => call.Position)
            ]);
    }

    private static async Task<PresentationBoundaryDocumentAnalysis[]>
        GetDocumentAnalysisAsync(
            string repositoryRoot,
            Document document)
    {
        var presentationAssemblyName = document.Project.AssemblyName;

        if (presentationAssemblyName?.EndsWith(
                PresentationAssemblySuffix,
                StringComparison.Ordinal) != true)
        {
            return [];
        }

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
        var applicationAssemblyName =
            presentationAssemblyName[..^PresentationAssemblySuffix.Length] +
            ApplicationAssemblySuffix;

        return
        [
            new PresentationBoundaryDocumentAnalysis(
                RelativePath: relativePath,
                ApplicationReferences: GetApplicationReferences(
                    relativePath,
                    applicationAssemblyName,
                    semanticModel,
                    root),
                MediatorCalls: GetMediatorCalls(
                    relativePath,
                    presentationAssemblyName,
                    semanticModel,
                    root))
        ];
    }

    private static ApplicationReferenceSource[] GetApplicationReferences(
        string relativePath,
        string applicationAssemblyName,
        SemanticModel semanticModel,
        SyntaxNode root)
    {
        return
        [
            .. root
                .DescendantNodes()
                .OfType<SimpleNameSyntax>()
                .Where(name => !name.Ancestors().OfType<UsingDirectiveSyntax>()
                    .Any())
                .Where(name => !IsInsideMapper(name))
                .Select(name => new
                {
                    Name = name,
                    Symbol = GetReferencedSymbol(
                        semanticModel,
                        name)
                })
                .Where(target => string.Equals(
                    target.Symbol?.ContainingAssembly?.Name,
                    applicationAssemblyName,
                    StringComparison.Ordinal))
                .Select(target => new ApplicationReferenceSource(
                    RelativePath: relativePath,
                    LineNumber: GetLineNumber(target.Name),
                    Position: target.Name.SpanStart,
                    Expression: target.Name.ToString(),
                    ApplicationAssemblyName: applicationAssemblyName))
        ];
    }

    private static MediatorCallSource[] GetMediatorCalls(
        string relativePath,
        string presentationAssemblyName,
        SemanticModel semanticModel,
        SyntaxNode root)
    {
        return
        [
            .. root
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(invocation => new
                {
                    Invocation = invocation,
                    Operation = semanticModel.GetOperation(invocation) as
                        IInvocationOperation,
                    Endpoint = invocation.Ancestors()
                        .OfType<TypeDeclarationSyntax>()
                        .Select(declaration =>
                            semanticModel.GetDeclaredSymbol(declaration))
                        .OfType<INamedTypeSymbol>()
                        .FirstOrDefault(IsEndpoint)
                })
                .Where(target =>
                    target.Operation is not null &&
                    target.Endpoint is not null &&
                    MediatorMethodNames.Contains(
                        target.Operation.TargetMethod.Name,
                        StringComparer.Ordinal) &&
                    string.Equals(
                        target.Operation.TargetMethod.ContainingType
                            .ToDisplayString(),
                        MediatorTypeName,
                        StringComparison.Ordinal))
                .Select(target => new MediatorCallSource(
                    RelativePath: relativePath,
                    LineNumber: GetLineNumber(target.Invocation),
                    Position: target.Invocation.SpanStart,
                    EndpointTypeName: target.Endpoint!.ToDisplayString(),
                    MethodName: target.Operation!.TargetMethod.Name,
                    UsesPresentationMapper: IsPresentationMapperInvocation(
                        target.Operation.Arguments[0].Value,
                        presentationAssemblyName)))
        ];
    }

    private static ISymbol? GetReferencedSymbol(
        SemanticModel semanticModel,
        SimpleNameSyntax name)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(name);
        var symbol = symbolInfo.Symbol ??
                     symbolInfo.CandidateSymbols.FirstOrDefault();

        return symbol is IAliasSymbol alias
            ? alias.Target
            : symbol;
    }

    private static bool IsInsideMapper(SyntaxNode node)
    {
        return node.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .Any(declaration => declaration.Identifier.ValueText.EndsWith(
                MapperSuffix,
                StringComparison.Ordinal));
    }

    private static bool IsEndpoint(INamedTypeSymbol type)
    {
        return type.AllInterfaces.Any(interfaceType =>
            string.Equals(
                interfaceType.Name,
                "IEndpoint",
                StringComparison.Ordinal) &&
            string.Equals(
                interfaceType.ContainingNamespace.ToDisplayString(),
                EndpointInterfaceNamespace,
                StringComparison.Ordinal));
    }

    private static bool IsPresentationMapperInvocation(
        IOperation operation,
        string presentationAssemblyName)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation is IInvocationOperation mapperInvocation &&
               mapperInvocation.TargetMethod.ContainingType.Name.EndsWith(
                   MapperSuffix,
                   StringComparison.Ordinal) &&
               string.Equals(
                   mapperInvocation.TargetMethod.ContainingAssembly.Name,
                   presentationAssemblyName,
                   StringComparison.Ordinal);
    }

    private static int GetLineNumber(SyntaxNode node)
    {
        return node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
    }
}

internal sealed record PresentationBoundaryAnalysis(
    string[] PresentationDocuments,
    ApplicationReferenceSource[] ApplicationReferences,
    MediatorCallSource[] MediatorCalls);

internal sealed record PresentationBoundaryDocumentAnalysis(
    string RelativePath,
    ApplicationReferenceSource[] ApplicationReferences,
    MediatorCallSource[] MediatorCalls);

internal sealed record ApplicationReferenceSource(
    string RelativePath,
    int LineNumber,
    int Position,
    string Expression,
    string ApplicationAssemblyName);

internal sealed record MediatorCallSource(
    string RelativePath,
    int LineNumber,
    int Position,
    string EndpointTypeName,
    string MethodName,
    bool UsesPresentationMapper);
