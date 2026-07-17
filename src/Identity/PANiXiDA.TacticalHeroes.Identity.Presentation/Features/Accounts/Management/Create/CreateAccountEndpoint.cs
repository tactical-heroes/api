using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Create;

internal sealed class CreateAccountEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateAccount";
    public string Summary { get; } = "Create account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .Produces<CreateAccountResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        CreateAccountRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new CreateAccountCommand(
                request.Email,
                request.UserName,
                request.Password,
                request.IsConfirmed,
                [.. request.Claims.Select(Claim.ToApplicationClaim)],
                request.Status),
            cancellationToken);

        return result.ToHttpResult(id =>
            TypedResults.Created(
                $"/api/v1/accounts/{id}",
                new CreateAccountResponse(id)));
    }
}
