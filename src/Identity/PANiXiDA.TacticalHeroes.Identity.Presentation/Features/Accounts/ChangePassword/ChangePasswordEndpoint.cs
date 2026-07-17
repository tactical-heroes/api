using System.Security.Claims;

using Microsoft.AspNetCore.Http;

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

        if (!Guid.TryParse(input: accountIdValue, result: out var accountId))
        {
            return TypedResults.Unauthorized();
        }

        var result = await mediator.SendAsync(
            ChangePasswordMapper.ToCommand(request: request, accountId: accountId),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
