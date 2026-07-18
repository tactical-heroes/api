using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Unblock;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Unblock;

internal sealed class UnblockUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = $"{UsersEndpoints.IdRoute}/unblock";
    public string Name { get; } = "UnblockUser";
    public string Summary { get; } = "Unblock user";

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
            new UnblockUserCommand(Id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
