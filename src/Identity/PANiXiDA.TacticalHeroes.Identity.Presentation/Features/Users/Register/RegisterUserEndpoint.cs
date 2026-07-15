using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Register;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Register;

internal sealed class RegisterUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/register";
    public string Name { get; } = "RegisterUser";
    public string Summary { get; } = "Register identity user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(HandleAsync)
            .AllowAnonymous()
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        RegisterUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new RegisterUserCommand(request.Email, request.Password),
            cancellationToken);

        return result.ToHttpResult(userId =>
            TypedResults.Created(
                $"/api/v1/identity/auth/users/{userId}",
                new RegisterUserResponse(userId)));
    }
}
