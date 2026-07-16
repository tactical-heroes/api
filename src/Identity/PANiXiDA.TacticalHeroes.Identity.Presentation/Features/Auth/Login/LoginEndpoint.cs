using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

internal sealed class LoginEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/login";
    public string Name { get; } = "Login";
    public string Summary { get; } = "Log in user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status302Found)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(LoginRequest request)
    {
        return EndpointStub.NotImplemented(nameof(LoginEndpoint));
    }
}
