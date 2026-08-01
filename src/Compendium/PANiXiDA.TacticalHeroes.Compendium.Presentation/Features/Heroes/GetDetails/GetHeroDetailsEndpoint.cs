using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetDetails;

internal sealed class GetHeroDetailsEndpoint : IEndpoint<HeroesEndpoints>
{
    public string Route { get; } = HeroesEndpoints.IdRoute;
    public string Name { get; } = "GetHeroDetails";
    public string Summary { get; } = "Get hero";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleAsync)
            .Produces<GetHeroDetailsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetHeroDetailsMapper.ToQuery(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: hero =>
            TypedResults.Ok(
                value: GetHeroDetailsMapper.ToResponse(hero: hero)));
    }
}
