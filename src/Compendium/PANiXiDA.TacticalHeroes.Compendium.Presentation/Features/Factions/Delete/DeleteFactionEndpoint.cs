using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Delete;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Delete;

internal sealed class DeleteFactionEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = FactionsEndpoints.IdRoute;
    public string Name { get; } = "DeleteFaction";
    public string Summary { get; } = "Delete faction";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(Handle)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new DeleteFactionCommand(Id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
