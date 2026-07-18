using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Block;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Block;

internal sealed class BlockUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = $"{UsersEndpoints.IdRoute}/block";
    public string Name { get; } = "BlockUser";
    public string Summary { get; } = "Block user";

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
            new BlockUserCommand(Id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
