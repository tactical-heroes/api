using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Delete;

internal sealed class DeleteUnitEndpoint : IEndpoint<UnitsEndpoints>
{
    public string Route { get; } = UnitsEndpoints.IdRoute;
    public string Name { get; } = "DeleteUnit";
    public string Summary { get; } = "Delete unit";

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
            command: DeleteUnitMapper.ToCommand(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
