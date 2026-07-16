using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Delete;

internal sealed class DeleteRoleEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = RolesEndpoints.IdRoute;
    public string Name { get; } = "DeleteRole";
    public string Summary { get; } = "Delete role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(Guid id)
    {
        return EndpointStub.NotImplemented(nameof(DeleteRoleEndpoint));
    }
}
