using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetSections;

internal sealed class GetSectionsEndpoint : IEndpoint<ClaimsEndpoints>
{
    public string Route { get; } = "/sections";
    public string Name { get; } = "GetSections";
    public string Summary { get; } = "Get available sections";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<GetSectionsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetSectionsMapper.ToQuery(),
            cancellationToken);

        return result.ToHttpResult(onSuccess: sections =>
            TypedResults.Ok(value: GetSectionsMapper.ToResponse(sections: sections)));
    }
}
