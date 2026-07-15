using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset.Reset;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.PasswordReset.Reset;

internal sealed class ResetPasswordEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/password-reset/confirm";
    public string Name { get; } = "ResetIdentityPassword";
    public string Summary { get; } = "Reset identity password";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(HandleAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        ResetPasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new ResetPasswordCommand(
                request.UserId,
                request.PasswordResetToken,
                request.NewPassword),
            cancellationToken);

        return result.ToHttpResult(() => TypedResults.NoContent());
    }
}
