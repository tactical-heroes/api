using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetDetails;

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
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        RegisterAccountRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            RegisterAccountMapper.ToCommand(request),
            cancellationToken);

        return result.ToHttpResult(id =>
            TypedResults.CreatedAtRoute(
                RegisterAccountMapper.ToResponse(id),
                new GetAccountDetailsEndpoint().Name,
                new { id }));
    }
}
