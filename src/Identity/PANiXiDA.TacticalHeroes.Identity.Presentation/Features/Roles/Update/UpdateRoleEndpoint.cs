using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

internal sealed class UpdateRoleEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = RolesEndpoints.IdRoute;
    public string Name { get; } = "UpdateRole";
    public string Summary { get; } = "Update role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new UpdateRoleCommand(
                id,
                request.Name,
                [.. request.Claims.Select(Claim.ToApplicationClaim)]),
            cancellationToken);

        return result.ToHttpResult(TypedResults.NoContent);
    }
}
