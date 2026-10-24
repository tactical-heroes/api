using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Delete;

internal sealed class DeleteRoleEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = RolesEndpoints.IdRoute;
    public string Name { get; } = "DeleteRole";
    public string Summary { get; } = "Delete role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(builder.Route, HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        HttpContext httpContext,
        CurrentUserCookieSession cookieSession,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var session = await cookieSession.CaptureAsync(httpContext);
        var result = await mediator.SendAsync(
            DeleteRoleMapper.ToCommand(id: id),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        if (HttpCurrentUserSessionRefresh.IsRequired(httpContext))
        {
            await cookieSession.RenewAsync(
                context: httpContext,
                session: session,
                cancellationToken: cancellationToken);
        }

        return TypedResults.NoContent();
    }
}
