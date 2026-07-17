using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ForgotPassword;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ForgotPassword;

internal sealed class ForgotPasswordEndpoint : IEndpoint<AccountsEndpoints>
{
    public string Route { get; } = "/forgot-password";
    public string Name { get; } = "ForgotPassword";
    public string Summary { get; } = "Request password reset";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        ForgotPasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new ForgotPasswordCommand(request.Email),
            cancellationToken);

        return result.ToHttpResult(
            () => TypedResults.StatusCode(StatusCodes.Status202Accepted));
    }
}
