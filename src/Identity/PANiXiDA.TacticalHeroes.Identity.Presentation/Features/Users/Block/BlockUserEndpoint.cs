using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Block;

internal sealed class BlockUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = $"{UsersEndpoints.IdRoute}/block";
    public string Name { get; } = "BlockUser";
    public string Summary { get; } = "Block user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            BlockUserMapper.ToCommand(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
