using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Unblock;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Unblock;

internal sealed class UnblockAccountEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = $"{AccountManagementEndpoints.IdRoute}/unblock";
    public string Name { get; } = "UnblockAccount";
    public string Summary { get; } = "Unblock account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
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
            new UnblockAccountCommand(Id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
