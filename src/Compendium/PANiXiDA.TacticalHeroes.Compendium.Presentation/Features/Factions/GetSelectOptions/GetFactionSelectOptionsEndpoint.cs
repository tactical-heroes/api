using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetSelectOptions;

internal sealed class GetFactionSelectOptionsEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = "/select-options";
    public string Name { get; } = "GetFactionSelectOptions";
    public string Summary { get; } = "Get faction select options";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleAsync)
            .Produces<IReadOnlyList<FactionSelectOptionResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] GetFactionSelectOptionsRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetFactionSelectOptionsMapper.ToQuery(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: options =>
            TypedResults.Ok(value: GetFactionSelectOptionsMapper.ToResponse(options: options)));
    }
}
