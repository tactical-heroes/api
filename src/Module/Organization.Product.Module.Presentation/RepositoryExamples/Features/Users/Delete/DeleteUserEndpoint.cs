using Microsoft.AspNetCore.Http;

using Organization.Product.Module.Application.Users.Delete;

namespace Organization.Product.Module.Presentation.Features.Users.Delete;

internal class DeleteUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = $"/{UsersEndpoints.IdRoute}";
    public string Name { get; } = "DeleteUser";
    public string Summary { get; } = "Delete user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new DeleteUserCommand(id), cancellationToken);

        return result.ToHttpResult(TypedResults.NoContent);
    }
}
