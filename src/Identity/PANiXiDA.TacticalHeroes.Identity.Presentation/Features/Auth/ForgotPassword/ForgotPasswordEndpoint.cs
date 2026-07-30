using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ForgotPassword;

internal sealed class ForgotPasswordEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/forgot-password";
    public string Name { get; } = "ForgotPassword";
    public string Summary { get; } = "Request password reset";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(HandleAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        ForgotPasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            ForgotPasswordMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(
            onSuccess: () => TypedResults.StatusCode(statusCode: StatusCodes.Status202Accepted));
    }
}
