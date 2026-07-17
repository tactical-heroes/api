using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Confirm;

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
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        ConfirmAccountRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new ConfirmAccountCommand(
                request.UserId,
                request.EmailConfirmationToken),
            cancellationToken);

        return result.ToHttpResult(TypedResults.NoContent);
    }
}
