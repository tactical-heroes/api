using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Update;

internal sealed class UpdateHeroEndpoint : IEndpoint<HeroesEndpoints>
{
    public string Route { get; } = HeroesEndpoints.IdRoute;
    public string Name { get; } = "UpdateHero";
    public string Summary { get; } = "Update hero";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(pattern: builder.Route, handler: HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateHeroRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: UpdateHeroMapper.ToCommand(request: request, id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
