using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Presentation;

public sealed class EndpointBehaviorConventionTests
{
    private const string ApplicationNamespaceSegment = "Application";

    private static readonly string[] MediatorMethodNames =
    [
        "QueryAsync",
        "SendAsync"
    ];

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
    public void PresentationApplicationReferences_Should_ExistOnlyInMappers_When_Declared()
    {
        var presentationSources = PresentationArchitectureConvention
            .GetPresentationTypes(_ => true)
            .SelectMany(type =>
                PresentationArchitectureConvention.FindSourceFiles(type))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(sourceFile => new
            {
                SourceFile = sourceFile,
                Root = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree
                    .ParseText(File.ReadAllText(sourceFile))
                    .GetRoot()
            })
            .ToArray();
        var violations = presentationSources
            .Where(source => !DeclaresMapper(source.Root))
            .SelectMany(source => source.Root
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Where(usingDirective =>
                    usingDirective.Name?.ToString()
                        .Split('.')
                        .Contains(
                            ApplicationNamespaceSegment,
                            StringComparer.Ordinal) == true)
                .Select(usingDirective =>
                    $"'{source.SourceFile}:{GetLineNumber(usingDirective)}' " +
                    $"references Application outside a mapper."))
            .ToArray();

        Assert.NotEmpty(presentationSources);
        Assert.True(
            violations.Length == 0,
            $"Presentation Application reference violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Mediator messages should be created by slice mappers when endpoint sends a message")]
    public void MediatorMessages_Should_BeCreatedBySliceMappers_When_EndpointSendsAMessage()
    {
        var mediatorCalls = MediatorMethodNames
            .SelectMany(GetEndpointInvocations)
            .ToArray();
        var violations = mediatorCalls
            .Where(call =>
                !IsMediatorInvocation(call.Invocation) ||
                !UsesMapperForMessage(call.Invocation))
            .Select(call =>
                $"{call.Endpoint.FullName}.{GetInvocationName(call.Invocation)} " +
                $"must call IMediator with a command or query created by its " +
                $"slice Mapper at '{Path.GetFileName(call.SourceFile)}:" +
                $"{GetLineNumber(call.Invocation)}'.")
            .ToArray();

        Assert.NotEmpty(mediatorCalls);
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

    private static bool IsMediatorInvocation(
        InvocationExpressionSyntax invocation)
    {
        return invocation.Expression is MemberAccessExpressionSyntax
        {
            Expression: IdentifierNameSyntax
            {
                Identifier.ValueText: "mediator"
            }
        };
    }

    private static bool UsesMapperForMessage(
        InvocationExpressionSyntax invocation)
    {
        var messageArgument = invocation.ArgumentList.Arguments.FirstOrDefault();

        return messageArgument?.Expression is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Expression: IdentifierNameSyntax mapperName
            }
        } &&
        mapperName.Identifier.ValueText.EndsWith(
            "Mapper",
            StringComparison.Ordinal);
    }

    private static bool DeclaresMapper(
        Microsoft.CodeAnalysis.SyntaxNode root)
    {
        return root
            .DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .Any(declaration => declaration.Identifier.ValueText.EndsWith(
                "Mapper",
                StringComparison.Ordinal));
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
