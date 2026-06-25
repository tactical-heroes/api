using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.PasswordReset;

internal sealed class RequestPasswordResetEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/password-reset/request";
    public string Name { get; } = "RequestIdentityPasswordReset";
    public string Summary { get; } = "Request identity password reset";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(HandleAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        RequestPasswordResetRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new RequestPasswordResetCommand(request.Email),
            cancellationToken);

        return result.ToHttpResult(() => TypedResults.StatusCode(StatusCodes.Status202Accepted));
    }
}
