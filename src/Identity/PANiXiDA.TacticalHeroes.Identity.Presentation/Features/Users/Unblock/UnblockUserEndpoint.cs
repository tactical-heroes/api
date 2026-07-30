using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Unblock;

internal sealed class UnblockUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = $"{UsersEndpoints.IdRoute}/unblock";
    public string Name { get; } = "UnblockUser";
    public string Summary { get; } = "Unblock user";

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
            command: UnblockUserMapper.ToCommand(id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
