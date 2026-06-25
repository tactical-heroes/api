using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Register;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Register;

internal sealed class RegisterUserEndpoint : IEndpoint<IdentityUsersEndpoints>
{
    public string Route { get; } = "/register";
    public string Name { get; } = "RegisterIdentityUser";
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

        return result.ToHttpResult(registerResult =>
            TypedResults.Created(
                $"/api/v1/identity/auth/users/{registerResult.UserId}",
                new RegisterUserResponse(registerResult.UserId)));
    }
}
