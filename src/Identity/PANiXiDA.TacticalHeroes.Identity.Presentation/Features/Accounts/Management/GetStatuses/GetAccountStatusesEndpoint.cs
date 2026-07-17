using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetStatuses;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetStatuses;

internal sealed class GetAccountStatusesEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = "/statuses";
    public string Name { get; } = "GetAccountStatuses";
    public string Summary { get; } = "Get account statuses";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<IReadOnlyCollection<AccountStatusResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            new GetAccountStatusesQuery(),
            cancellationToken);

        return result.ToHttpResult(statuses =>
            TypedResults.Ok(GetAccountStatusesMapper.ToResponse(statuses)));
    }
}
