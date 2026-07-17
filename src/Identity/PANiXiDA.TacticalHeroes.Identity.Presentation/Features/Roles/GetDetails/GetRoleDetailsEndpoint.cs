using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetDetails;

internal sealed class GetRoleDetailsEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = RolesEndpoints.IdRoute;
    public string Name { get; } = "GetRoleDetails";
    public string Summary { get; } = "Get role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<GetRoleDetailsResponse>(StatusCodes.Status200OK)
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
            new GetRoleDetailsQuery(id),
            cancellationToken);

        return result.ToHttpResult(role =>
            TypedResults.Ok(GetRoleDetailsMapper.ToResponse(role)));
    }
}
