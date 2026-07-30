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
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        RegisterUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            RegisterUserMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: RegisterUserMapper.ToResponse(id: id),
                routeName: new GetUserDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
