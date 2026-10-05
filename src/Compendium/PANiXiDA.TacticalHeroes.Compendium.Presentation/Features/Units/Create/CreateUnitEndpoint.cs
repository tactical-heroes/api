using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Create;

internal sealed class CreateUnitEndpoint : IEndpoint<UnitsEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateUnit";
    public string Summary { get; } = "Create unit";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(pattern: builder.Route, handler: HandleAsync)
            .Produces<CreateUnitResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        CreateUnitRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: CreateUnitMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateUnitMapper.ToResponse(id: id),
                routeName: new GetUnitDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
