using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;

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
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAccountsRequest request,
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            new GetAccountsQuery(request.Email, pagination),
            cancellationToken);

        return result.ToHttpResult(page =>
            TypedResults.Ok(
                PaginationResult<AccountListItemResponse>.Create(
                    page.Items.Select(item => new AccountListItemResponse(
                        item.Id,
                        item.Email,
                        item.UserName,
                        item.IsConfirmed,
                        item.Status,
                        item.StatusDisplayName)),
                    page.PageNumber,
                    page.PageSize,
                    page.TotalCount)));
    }
}
