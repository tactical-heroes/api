using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

internal sealed class ResetPasswordEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/reset-password";
    public string Name { get; } = "ResetPassword";
    public string Summary { get; } = "Reset password";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(pattern: builder.Route, handler: HandleAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        ResetPasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: ResetPasswordMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
