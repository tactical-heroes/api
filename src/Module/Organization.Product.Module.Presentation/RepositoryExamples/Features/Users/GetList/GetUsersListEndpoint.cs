using Microsoft.AspNetCore.Http;

using Organization.Product.Module.Application.Users.GetList;

namespace Organization.Product.Module.Presentation.Features.Users.GetList;

internal sealed class GetUsersListEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetUserList";
    public string Summary { get; } = "Get paginated user list";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleAsync)
            .Produces<PaginationResult<UserListItemResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] GetUsersListRequest request,
        [AsParameters] PaginationParameters paginationParameters,
        [AsParameters] SortParameters sortParameters,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var filterParameters = GetUsersListMapper.ToFilterParameters(request);

        var result = await mediator.QueryAsync(
            new GetUsersListQuery(filterParameters, paginationParameters, sortParameters),
            cancellationToken);

        return result.ToHttpResult(item
            => TypedResults.Ok(GetUsersListMapper.ToResponse(item)));
    }
}
