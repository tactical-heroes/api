using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetList;

internal sealed class GetHeroesEndpoint : IEndpoint<HeroesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetHeroes";
    public string Summary { get; } = "Get heroes";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleAsync)
            .Produces<PaginationResult<HeroListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] PaginationParameters pagination,
        [AsParameters] SortingParameters sorting,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetHeroesMapper.ToQuery(pagination: pagination,
                sorting: sorting),
            cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetHeroesMapper.ToResponse(page: page)));
    }
}
