using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetList;

internal sealed class GetRolesEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetRoles";
    public string Summary { get; } = "Get roles";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(handler: Handle)
            .Produces<PaginationResult<RoleListItemResponse>>(statusCode: StatusCodes.Status200OK)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            query: GetRolesMapper.ToQuery(pagination: pagination),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetRolesMapper.ToResponse(page: page)));
    }
}
