using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetDetails;

internal sealed class GetUserDetailsEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = UsersEndpoints.IdRoute;
    public string Name { get; } = "GetUserDetails";
    public string Summary { get; } = "Get user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(handler: Handle)
            .Produces<GetUserDetailsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            query: GetUserDetailsMapper.ToQuery(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: user =>
            TypedResults.Ok(value: GetUserDetailsMapper.ToResponse(user: user)));
    }
}
