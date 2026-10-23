using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/resend-confirmation-email";
    public string Name { get; } = "ResendConfirmationEmail";
    public string Summary { get; } = "Resend email confirmation email";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.ResendConfirmationEmail)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        ResendConfirmationEmailRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            ResendConfirmationEmailMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
