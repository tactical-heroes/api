using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetList;

internal sealed class GetFactionsEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetFactions";
    public string Summary { get; } = "Get factions";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleAsync)
            .Produces<PaginationResult<FactionListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetFactionsMapper.ToQuery(pagination: pagination),
            cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetFactionsMapper.ToResponse(page: page)));
    }
}
