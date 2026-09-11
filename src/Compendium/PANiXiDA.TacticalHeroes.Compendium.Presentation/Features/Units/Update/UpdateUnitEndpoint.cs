using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Update;

internal sealed class UpdateUnitEndpoint : IEndpoint<UnitsEndpoints>
{
    public string Route { get; } = UnitsEndpoints.IdRoute;
    public string Name { get; } = "UpdateUnit";
    public string Summary { get; } = "Update unit";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateUnitRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            UpdateUnitMapper.ToCommand(request: request, id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
