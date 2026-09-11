using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Delete;

internal sealed class DeleteUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = UsersEndpoints.IdRoute;
    public string Name { get; } = "DeleteUser";
    public string Summary { get; } = "Delete user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(HandleAsync)
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
            DeleteUserMapper.ToCommand(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
