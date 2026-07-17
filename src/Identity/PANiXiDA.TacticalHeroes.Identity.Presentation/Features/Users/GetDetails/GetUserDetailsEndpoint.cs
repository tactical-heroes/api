using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetDetails;

internal sealed class GetUserDetailsEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = UsersEndpoints.IdRoute;
    public string Name { get; } = "GetUserDetails";
    public string Summary { get; } = "Get user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<GetUserDetailsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            new GetUserDetailsQuery(Id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: user =>
            TypedResults.Ok(value: GetUserDetailsMapper.ToResponse(user: user)));
    }
}
