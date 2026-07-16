using Microsoft.AspNetCore.Http;

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
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(Guid id, UpdateAccountRequest request)
    {
        return EndpointStub.NotImplemented(nameof(UpdateAccountEndpoint));
    }
}
