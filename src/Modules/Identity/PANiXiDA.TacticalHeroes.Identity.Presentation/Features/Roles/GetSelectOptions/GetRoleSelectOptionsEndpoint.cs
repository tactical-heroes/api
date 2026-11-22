using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetSelectOptions;

internal sealed class GetRoleSelectOptionsEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = "/select-options";
    public string Name { get; } = "GetRoleSelectOptions";
    public string Summary { get; } = "Get role select options";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<IReadOnlyList<RoleSelectOptionResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> HandleAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(GetRoleSelectOptionsMapper.ToQuery(), cancellationToken);

        return result.ToHttpResult(onSuccess: options =>
            TypedResults.Ok(value: GetRoleSelectOptionsMapper.ToResponse(options: options)));
    }
}
