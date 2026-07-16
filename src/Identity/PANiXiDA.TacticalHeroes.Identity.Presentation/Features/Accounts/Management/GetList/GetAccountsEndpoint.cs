using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetList;

internal sealed class GetAccountsEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetAccounts";
    public string Summary { get; } = "Get accounts";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<PaginationResult<AccountListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(
        [AsParameters] GetAccountsRequest request,
        [AsParameters] PaginationParameters pagination)
    {
        return EndpointStub.NotImplemented(nameof(GetAccountsEndpoint));
    }
}
