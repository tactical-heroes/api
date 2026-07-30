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
        builder.MapPost(HandleAsync)
            .RequireAuthorization()
            .Produces<CreateFactionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        CreateFactionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            CreateFactionMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateFactionMapper.ToResponse(id: id),
                routeName: new GetFactionDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
