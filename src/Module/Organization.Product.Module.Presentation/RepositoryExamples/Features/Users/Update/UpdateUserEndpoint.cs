using Microsoft.AspNetCore.Http;

namespace Organization.Product.Module.Presentation.Features.Users.Update;

internal sealed class UpdateUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = $"/{UsersEndpoints.IdRoute}";
    public string Name { get; } = "UpdateUser";
    public string Summary { get; } = "Update user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = UpdateUserMapper.ToCommand(request, id);
        var result = await mediator.SendAsync(command, cancellationToken);

        return result.ToHttpResult(TypedResults.NoContent);
    }
}
