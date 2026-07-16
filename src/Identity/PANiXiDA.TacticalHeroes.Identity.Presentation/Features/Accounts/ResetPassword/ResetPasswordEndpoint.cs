using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ResetPassword;

internal sealed class ResetPasswordEndpoint : IEndpoint<AccountsEndpoints>
{
    public string Route { get; } = "/reset-password";
    public string Name { get; } = "ResetPassword";
    public string Summary { get; } = "Reset password";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(ResetPasswordRequest request)
    {
        return EndpointStub.NotImplemented(nameof(ResetPasswordEndpoint));
    }
}
