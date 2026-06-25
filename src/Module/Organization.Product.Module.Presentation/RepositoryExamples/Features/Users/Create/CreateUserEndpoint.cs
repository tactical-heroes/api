using Microsoft.AspNetCore.Http;

using Organization.Product.Module.Presentation.Features.Users.GetDetails;

namespace Organization.Product.Module.Presentation.Features.Users.Create;

internal class CreateUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateUser";
    public string Summary { get; } = "Create user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(HandleAsync)
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = CreateUserMapper.ToCommand(request);
        var result = await mediator.SendAsync(command, cancellationToken);

        return result.ToHttpResult(createdId =>
            TypedResults.CreatedAtRoute(
                CreateUserMapper.ToResponse(createdId),
                new GetUserDetailsEndpoint().Name,
                new { id = createdId }));
    }
}
