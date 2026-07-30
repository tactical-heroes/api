using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetDetails;

internal sealed class GetRoleDetailsEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = RolesEndpoints.IdRoute;
    public string Name { get; } = "GetRoleDetails";
    public string Summary { get; } = "Get role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(handler: Handle)
            .Produces<GetRoleDetailsResponse>(statusCode: StatusCodes.Status200OK)
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
            query: GetRoleDetailsMapper.ToQuery(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: role =>
            TypedResults.Ok(value: GetRoleDetailsMapper.ToResponse(role: role)));
    }
}
