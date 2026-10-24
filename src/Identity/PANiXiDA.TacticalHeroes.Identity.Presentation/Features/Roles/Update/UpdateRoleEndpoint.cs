using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

internal sealed class UpdateRoleEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = RolesEndpoints.IdRoute;
    public string Name { get; } = "UpdateRole";
    public string Summary { get; } = "Update role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(builder.Route, HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateRoleRequest request,
        HttpContext httpContext,
        CurrentUserCookieSession cookieSession,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var session = await cookieSession.CaptureAsync(httpContext);
        var result = await mediator.SendAsync(
            UpdateRoleMapper.ToCommand(request: request, id: id),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        await cookieSession.RenewIfRequiredAsync(
            context: httpContext,
            session: session,
            cancellationToken: cancellationToken);

        return TypedResults.NoContent();
    }
}
