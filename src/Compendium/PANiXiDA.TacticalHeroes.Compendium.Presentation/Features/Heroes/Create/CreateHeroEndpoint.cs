using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Create;

internal sealed class CreateHeroEndpoint : IEndpoint<HeroesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateHero";
    public string Summary { get; } = "Create hero";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(pattern: builder.Route, handler: HandleAsync)
            .Produces<CreateHeroResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        CreateHeroRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: CreateHeroMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateHeroMapper.ToResponse(id: id),
                routeName: new GetHeroDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
