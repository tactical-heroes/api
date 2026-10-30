using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Cancel;

internal sealed class CancelMatchSearchEndpoint : IEndpoint<MatchSearchesEndpoints>
{
    public string Route { get; } = "/{id:guid}/cancel";
    public string Name { get; } = "CancelMatchSearch";
    public string Summary { get; } = "Cancel match search";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(CancelMatchSearchMapper.ToCommand(id), cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
