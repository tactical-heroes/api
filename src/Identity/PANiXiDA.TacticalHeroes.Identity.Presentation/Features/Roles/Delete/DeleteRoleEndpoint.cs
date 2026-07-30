using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Delete;

internal sealed class DeleteRoleEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = RolesEndpoints.IdRoute;
    public string Name { get; } = "DeleteRole";
    public string Summary { get; } = "Delete role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(handler: Handle)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: DeleteRoleMapper.ToCommand(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
