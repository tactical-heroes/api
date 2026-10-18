using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Update;

internal sealed class UpdateFactionEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = FactionsEndpoints.IdRoute;
    public string Name { get; } = "UpdateFaction";
    public string Summary { get; } = "Update faction";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(builder.Route, HandleAsync)
            .ProducesUpdateOrDeleteResponses();
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateFactionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            UpdateFactionMapper.ToCommand(request: request, id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
