using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Delete;

internal sealed class DeleteFactionEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = FactionsEndpoints.IdRoute;
    public string Name { get; } = "DeleteFaction";
    public string Summary { get; } = "Delete faction";

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
            command: DeleteFactionMapper.ToCommand(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
