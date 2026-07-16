using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Block;

internal sealed class BlockAccountEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = $"{AccountManagementEndpoints.IdRoute}/block";
    public string Name { get; } = "BlockAccount";
    public string Summary { get; } = "Block account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(Guid id)
    {
        return EndpointStub.NotImplemented(nameof(BlockAccountEndpoint));
    }
}
