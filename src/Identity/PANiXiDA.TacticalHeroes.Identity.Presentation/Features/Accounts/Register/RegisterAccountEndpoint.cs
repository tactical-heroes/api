using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Register;

internal sealed class RegisterAccountEndpoint : IEndpoint<AccountsEndpoints>
{
    public string Route { get; } = "/register";
    public string Name { get; } = "RegisterAccount";
    public string Summary { get; } = "Register account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces<RegisterAccountResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle(RegisterAccountRequest request)
    {
        return EndpointStub.NotImplemented(nameof(RegisterAccountEndpoint));
    }
}
