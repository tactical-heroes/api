using Microsoft.AspNetCore.Http;

using Organization.Product.Module.Application.Users.GetDetails;

namespace Organization.Product.Module.Presentation.Features.Users.GetDetails;

internal class GetUserDetailsEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = $"/{UsersEndpoints.IdRoute}";
    public string Name { get; } = "GetUserById";
    public string Summary { get; } = "Get user by id";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleAsync)
            .Produces<UserDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            new GetUserDetailsQuery(id), cancellationToken);

        return result.ToHttpResult(dto =>
            TypedResults.Ok(UserDetailsMapper.ToResponse(dto)));
    }
}
