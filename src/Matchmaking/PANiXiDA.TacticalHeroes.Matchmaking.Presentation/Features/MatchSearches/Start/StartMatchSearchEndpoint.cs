using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Start;

internal sealed class StartMatchSearchEndpoint : IEndpoint<MatchSearchesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "StartMatchSearch";
    public string Summary { get; } = "Start match search";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .Produces<StartMatchSearchResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        StartMatchSearchRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(StartMatchSearchMapper.ToCommand(request), cancellationToken);

        return result.ToHttpResult(onSuccess: search =>
            TypedResults.Created(uri: (string?)null, value: StartMatchSearchMapper.ToResponse(search)));
    }
}
