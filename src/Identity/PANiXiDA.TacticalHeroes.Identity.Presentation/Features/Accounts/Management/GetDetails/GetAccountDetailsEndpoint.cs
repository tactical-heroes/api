using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetDetails;

internal sealed class GetAccountDetailsEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = AccountManagementEndpoints.IdRoute;
    public string Name { get; } = "GetAccountDetails";
    public string Summary { get; } = "Get account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<GetAccountDetailsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            new GetAccountDetailsQuery(id),
            cancellationToken);

        return result.ToHttpResult(account =>
            TypedResults.Ok(
                new GetAccountDetailsResponse(
                    account.Id,
                    account.Email,
                    account.UserName,
                    account.IsConfirmed,
                    account.Status,
                    account.StatusDisplayName,
                    [.. account.Claims.Select(Claim.FromApplicationClaim)])));
    }
}
