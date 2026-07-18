using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Create;

internal sealed class CreateUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateUser";
    public string Summary { get; } = "Create user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            CreateUserMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateUserMapper.ToResponse(id: id),
                routeName: new GetUserDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
