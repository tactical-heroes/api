using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

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
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle()
    {
        return EndpointStub.NotImplemented(nameof(GetAccountStatusesEndpoint));
    }
}
