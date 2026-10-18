using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Delete;

internal sealed class DeleteUnitEndpoint : IEndpoint<UnitsEndpoints>
{
    public string Route { get; } = UnitsEndpoints.IdRoute;
    public string Name { get; } = "DeleteUnit";
    public string Summary { get; } = "Delete unit";

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
            DeleteUnitMapper.ToCommand(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
