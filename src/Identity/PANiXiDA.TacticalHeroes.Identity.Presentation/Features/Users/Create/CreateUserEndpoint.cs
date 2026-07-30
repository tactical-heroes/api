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
        builder.MapPost(handler: Handle)
            .Produces<CreateUserResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: CreateUserMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateUserMapper.ToResponse(id: id),
                routeName: new GetUserDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
