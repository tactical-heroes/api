using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetList;

internal sealed class GetUsersEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetUsers";
    public string Summary { get; } = "Get users";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<PaginationResult<UserListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetUsersRequest request,
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            new GetUsersQuery(Email: request.Email, Pagination: pagination),
            cancellationToken);

        return result.ToHttpResult(onSuccess: page =>
            TypedResults.Ok(value: GetUsersMapper.ToResponse(page: page)));
    }
}
