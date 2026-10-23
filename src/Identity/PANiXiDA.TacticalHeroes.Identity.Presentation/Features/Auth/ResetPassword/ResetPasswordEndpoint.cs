using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

internal sealed class ResetPasswordEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/reset-password";
    public string Name { get; } = "ResetPassword";
    public string Summary { get; } = "Reset password";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.ResetPassword)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        ResetPasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            ResetPasswordMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
