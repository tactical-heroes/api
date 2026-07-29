using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetStatuses;

internal sealed class GetUserStatusesEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/statuses";
    public string Name { get; } = "GetUserStatuses";
    public string Summary { get; } = "Get user statuses";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .Produces<IReadOnlyCollection<UserStatusResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetUserStatusesMapper.ToQuery(),
            cancellationToken);

        return result.ToHttpResult(onSuccess: statuses =>
            TypedResults.Ok(value: GetUserStatusesMapper.ToResponse(statuses: statuses)));
    }
}
