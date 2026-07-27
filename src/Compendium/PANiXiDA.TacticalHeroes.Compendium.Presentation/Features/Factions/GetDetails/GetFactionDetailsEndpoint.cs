using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetDetails;

internal sealed class GetFactionDetailsEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = FactionsEndpoints.IdRoute;
    public string Name { get; } = "GetFactionDetails";
    public string Summary { get; } = "Get faction";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<GetFactionDetailsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            new GetFactionDetailsQuery(Id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: faction =>
            TypedResults.Ok(
                value: GetFactionDetailsMapper.ToResponse(faction: faction)));
    }
}
