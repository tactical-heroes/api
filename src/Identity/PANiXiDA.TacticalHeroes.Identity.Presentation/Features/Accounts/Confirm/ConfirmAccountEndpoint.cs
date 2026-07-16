using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Confirm;

internal sealed class ConfirmAccountEndpoint : IEndpoint<AccountsEndpoints>
{
    public string Route { get; } = "/confirm";
    public string Name { get; } = "ConfirmAccount";
    public string Summary { get; } = "Confirm account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(ConfirmAccountRequest request)
    {
        return EndpointStub.NotImplemented(nameof(ConfirmAccountEndpoint));
    }
}
