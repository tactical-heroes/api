using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

internal sealed class CreateFactionEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateFaction";
    public string Summary { get; } = "Create faction";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .RequireAuthorization()
            .Produces<CreateFactionResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        CreateFactionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: CreateFactionMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateFactionMapper.ToResponse(id: id),
                routeName: new GetFactionDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
