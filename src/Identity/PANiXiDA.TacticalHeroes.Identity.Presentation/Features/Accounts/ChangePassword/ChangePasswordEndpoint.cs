using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ChangePassword;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ChangePassword;

internal sealed class ChangePasswordEndpoint : IEndpoint<AccountsEndpoints>
{
    public string Route { get; } = "/change-password";
    public string Name { get; } = "ChangePassword";
    public string Summary { get; } = "Change current user password";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var accountIdValue = user.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;

        if (!Guid.TryParse(accountIdValue, out var accountId))
        {
            return TypedResults.Unauthorized();
        }

        var result = await mediator.SendAsync(
            new ChangePasswordCommand(
                accountId,
                request.CurrentPassword,
                request.NewPassword),
            cancellationToken);

        return result.ToHttpResult(TypedResults.NoContent);
    }
}
