using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Permissions.GetList;

internal sealed class GetPermissionsEndpoint : IEndpoint<PermissionsEndpoints>
{
    public string Route { get; } = string.Empty;
    public string Name { get; } = "GetPermissions";
    public string Summary { get; } = "Get available permissions";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<IReadOnlyCollection<PermissionResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetPermissionsMapper.ToQuery(),
            cancellationToken);

        return result.ToHttpResult(onSuccess: permissions =>
            TypedResults.Ok(value: GetPermissionsMapper.ToResponse(permissions: permissions)));
    }
}
