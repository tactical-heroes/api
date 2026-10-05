using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

internal sealed class UpdateUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = UsersEndpoints.IdRoute;
    public string Name { get; } = "UpdateUser";
    public string Summary { get; } = "Update user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(pattern: builder.Route, handler: HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: UpdateUserMapper.ToCommand(request: request, id: id),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
