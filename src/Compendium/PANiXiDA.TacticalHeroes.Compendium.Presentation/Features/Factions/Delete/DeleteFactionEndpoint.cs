using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Delete;

internal sealed class DeleteFactionEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = FactionsEndpoints.IdRoute;
    public string Name { get; } = "DeleteFaction";
    public string Summary { get; } = "Delete faction";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(builder.Route, HandleAsync)
            .ProducesUpdateOrDeleteResponses();
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            DeleteFactionMapper.ToCommand(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
