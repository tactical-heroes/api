using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

internal sealed class UpdateUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = UsersEndpoints.IdRoute;
    public string Name { get; } = "UpdateUser";
    public string Summary { get; } = "Update user";

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
        UpdateUserRequest request,
        HttpContext httpContext,
        CurrentUserCookieSession cookieSession,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var session = await cookieSession.CaptureAsync(httpContext, targetUserId: id);
        var result = await mediator.SendAsync(
            UpdateUserMapper.ToCommand(request: request, id: id),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        await cookieSession.RenewAsync(
            context: httpContext,
            session: session,
            cancellationToken: cancellationToken);

        return TypedResults.NoContent();
    }
}
