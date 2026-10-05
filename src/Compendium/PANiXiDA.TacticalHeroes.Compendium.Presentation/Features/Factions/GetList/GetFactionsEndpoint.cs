using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetList;

internal sealed class GetFactionsEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetFactions";
    public string Summary { get; } = "Get factions";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(pattern: builder.Route, handler: HandleAsync)
            .Produces<PaginationResult<FactionListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] PaginationParameters paginationParameters,
        [AsParameters] SortingParameters sortingParameters,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            query: GetFactionsMapper.ToQuery(
                paginationParameters: paginationParameters,
                sortingParameters: sortingParameters),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetFactionsMapper.ToResponse(page: page)));
    }
}
