using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Update;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Update;

internal sealed class UpdateAccountEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = AccountManagementEndpoints.IdRoute;
    public string Name { get; } = "UpdateAccount";
    public string Summary { get; } = "Update account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateAccountRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new UpdateAccountCommand(
                id,
                request.Email,
                request.UserName,
                request.IsConfirmed,
                [.. request.Claims.Select(Claim.ToApplicationClaim)],
                request.Status),
            cancellationToken);

        return result.ToHttpResult(TypedResults.NoContent);
    }
}
