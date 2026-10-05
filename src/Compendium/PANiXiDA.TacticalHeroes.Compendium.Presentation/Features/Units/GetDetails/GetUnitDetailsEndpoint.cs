using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.GetDetails;

internal sealed class GetUnitDetailsEndpoint : IEndpoint<UnitsEndpoints>
{
    public string Route { get; } = UnitsEndpoints.IdRoute;
    public string Name { get; } = "GetUnitDetails";
    public string Summary { get; } = "Get unit";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(pattern: builder.Route, handler: HandleAsync)
            .Produces<GetUnitDetailsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            query: GetUnitDetailsMapper.ToQuery(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: unit =>
            TypedResults.Ok(
                value: GetUnitDetailsMapper.ToResponse(unit: unit)));
    }
}
