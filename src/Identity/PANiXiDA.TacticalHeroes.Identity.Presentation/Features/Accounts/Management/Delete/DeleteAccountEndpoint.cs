using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Delete;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Delete;

internal sealed class DeleteAccountEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = AccountManagementEndpoints.IdRoute;
    public string Name { get; } = "DeleteAccount";
    public string Summary { get; } = "Delete account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new DeleteAccountCommand(Id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
