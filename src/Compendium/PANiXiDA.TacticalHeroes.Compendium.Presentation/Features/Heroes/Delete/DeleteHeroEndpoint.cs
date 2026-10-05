using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Delete;

internal sealed class DeleteHeroEndpoint : IEndpoint<HeroesEndpoints>
{
    public string Route { get; } = HeroesEndpoints.IdRoute;
    public string Name { get; } = "DeleteHero";
    public string Summary { get; } = "Delete hero";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(pattern: builder.Route, handler: HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: DeleteHeroMapper.ToCommand(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
