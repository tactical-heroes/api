using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.GetList;

internal sealed class GetUnitsEndpoint : IEndpoint<UnitsEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetUnits";
    public string Summary { get; } = "Get units";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleAsync)
            .Produces<PaginationResult<UnitListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetUnitsMapper.ToQuery(pagination: pagination),
            cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetUnitsMapper.ToResponse(page: page)));
    }
}
