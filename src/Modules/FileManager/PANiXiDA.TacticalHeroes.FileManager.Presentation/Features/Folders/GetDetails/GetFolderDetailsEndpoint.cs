using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;

internal sealed class GetFolderDetailsEndpoint : IEndpoint<FoldersEndpoints>
{
    public string Route { get; } = FoldersEndpoints.IdRoute;
    public string Name { get; } = "GetFolderDetails";
    public string Summary { get; } = "Get folder";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<GetFolderDetailsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetFolderDetailsMapper.ToQuery(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: folder =>
            TypedResults.Ok(value: GetFolderDetailsMapper.ToResponse(folder: folder)));
    }
}
