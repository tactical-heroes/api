using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetList;

internal sealed class GetUsersEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetUsers";
    public string Summary { get; } = "Get users";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(pattern: builder.Route, handler: HandleAsync)
            .Produces<PaginationResult<UserListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] GetUsersRequest request,
        [AsParameters] PaginationParameters paginationParameters,
        [AsParameters] SortingParameters sortingParameters,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            query: GetUsersMapper.ToQuery(
                request: request,
                paginationParameters: paginationParameters,
                sortingParameters: sortingParameters),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetUsersMapper.ToResponse(page: page)));
    }
}
