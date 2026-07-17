using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

internal sealed class CreateRoleEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateRole";
    public string Summary { get; } = "Create role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .Produces<CreateRoleResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        CreateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new CreateRoleCommand(
                request.Name,
                [.. request.Claims.Select(Claim.ToApplicationClaim)]),
            cancellationToken);

        return result.ToHttpResult(id =>
            TypedResults.Created(
                $"/api/v1/roles/{id}",
                new CreateRoleResponse(id)));
    }
}
