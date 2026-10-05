using Microsoft.AspNetCore.Http;

using PANiXiDA.Core.Application.Querying.Limiting;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetSelectOptions;

internal sealed class GetFactionSelectOptionsEndpoint : IEndpoint<FactionsEndpoints>
{
    public string Route { get; } = "/select-options";
    public string Name { get; } = "GetFactionSelectOptions";
    public string Summary { get; } = "Get faction select options";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(pattern: builder.Route, handler: HandleAsync)
            .Produces<IReadOnlyList<FactionSelectOptionResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] GetFactionSelectOptionsRequest request,
        [AsParameters] LimitParameters limit,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            query: GetFactionSelectOptionsMapper.ToQuery(
                request: request,
                limit: limit),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: options =>
            TypedResults.Ok(value: GetFactionSelectOptionsMapper.ToResponse(options: options)));
    }
}
