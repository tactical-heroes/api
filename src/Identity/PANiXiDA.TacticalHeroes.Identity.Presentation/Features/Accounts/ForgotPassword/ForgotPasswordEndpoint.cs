using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ForgotPassword;

internal sealed class ForgotPasswordEndpoint : IEndpoint<AccountsEndpoints>
{
    public string Route { get; } = "/forgot-password";
    public string Name { get; } = "ForgotPassword";
    public string Summary { get; } = "Request password reset";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(ForgotPasswordRequest request)
    {
        return EndpointStub.NotImplemented(nameof(ForgotPasswordEndpoint));
    }
}
