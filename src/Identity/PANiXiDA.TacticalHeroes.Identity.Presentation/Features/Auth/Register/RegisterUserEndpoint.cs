using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;

internal sealed class RegisterUserEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/register";
    public string Name { get; } = "RegisterUser";
    public string Summary { get; } = "Register user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .AllowAnonymous()
            .Produces<RegisterUserResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        RegisterUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: RegisterUserMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: RegisterUserMapper.ToResponse(id: id),
                routeName: new GetUserDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
