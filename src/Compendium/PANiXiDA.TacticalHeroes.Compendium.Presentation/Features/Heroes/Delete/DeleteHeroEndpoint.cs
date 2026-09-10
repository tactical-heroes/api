using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Delete;

internal sealed class DeleteHeroEndpoint : IEndpoint<HeroesEndpoints>
{
    public string Route { get; } = HeroesEndpoints.IdRoute;
    public string Name { get; } = "DeleteHero";
    public string Summary { get; } = "Delete hero";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            DeleteHeroMapper.ToCommand(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
