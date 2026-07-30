using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetList;

internal sealed class GetUsersEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetUsers";
    public string Summary { get; } = "Get users";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(handler: Handle)
            .Produces<PaginationResult<UserListItemResponse>>(statusCode: StatusCodes.Status200OK)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .Produces(statusCode: StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetUsersRequest request,
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            query: GetUsersMapper.ToQuery(
                                   request: request,
                                   pagination: pagination),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetUsersMapper.ToResponse(page: page)));
    }
}
