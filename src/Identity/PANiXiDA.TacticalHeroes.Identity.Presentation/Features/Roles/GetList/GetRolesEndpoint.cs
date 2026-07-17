using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetList;

internal sealed class GetRolesEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetRoles";
    public string Summary { get; } = "Get roles";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<PaginationResult<RoleListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            new GetRolesQuery(Pagination: pagination),
            cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetRolesMapper.ToResponse(page: page)));
    }
}
