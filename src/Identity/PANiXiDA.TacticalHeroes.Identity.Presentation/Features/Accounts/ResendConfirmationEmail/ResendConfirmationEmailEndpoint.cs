using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailEndpoint : IEndpoint<AccountsEndpoints>
{
    public string Route { get; } = "/resend-confirmation-email";
    public string Name { get; } = "ResendConfirmationEmail";
    public string Summary { get; } = "Resend account confirmation email";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(ResendConfirmationEmailRequest request)
    {
        return EndpointStub.NotImplemented(nameof(ResendConfirmationEmailEndpoint));
    }
}
