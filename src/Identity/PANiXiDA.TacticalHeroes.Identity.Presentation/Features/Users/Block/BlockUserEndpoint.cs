using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Block;

internal sealed class BlockUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = $"{UsersEndpoints.IdRoute}/block";
    public string Name { get; } = "BlockUser";
    public string Summary { get; } = "Block user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: BlockUserMapper.ToCommand(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
