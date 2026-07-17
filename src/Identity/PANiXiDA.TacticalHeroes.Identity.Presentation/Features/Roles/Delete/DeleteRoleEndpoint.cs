using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Delete;

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
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new DeleteRoleCommand(Id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
