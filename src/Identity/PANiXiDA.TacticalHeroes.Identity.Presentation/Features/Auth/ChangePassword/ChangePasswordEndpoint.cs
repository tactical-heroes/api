using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;

internal sealed class ChangePasswordEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/change-password";
    public string Name { get; } = "ChangePassword";
    public string Summary { get; } = "Change current user password";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        ChangePasswordRequest request,
        HttpContext httpContext,
        CurrentUserCookieSession cookieSession,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var session = await cookieSession.CaptureAsync(httpContext);
        var result = await mediator.SendAsync(
            ChangePasswordMapper.ToCommand(request: request),
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
