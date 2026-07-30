using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetDetails;

internal sealed class GetFactionDetailsEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = FactionsEndpoints.IdRoute;
    public string Name { get; } = "GetFactionDetails";
    public string Summary { get; } = "Get faction";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(handler: Handle)
            .Produces<GetFactionDetailsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            query: GetFactionDetailsMapper.ToQuery(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: faction =>
            TypedResults.Ok(
                value: GetFactionDetailsMapper.ToResponse(faction: faction)));
    }
}
