using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

internal sealed class CreateRoleEndpoint : IEndpoint<RolesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateRole";
    public string Summary { get; } = "Create role";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .Produces<CreateRoleResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        CreateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: CreateRoleMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateRoleMapper.ToResponse(id: id),
                routeName: new GetRoleDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
