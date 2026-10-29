using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetList;

internal sealed class GetFoldersEndpoint : IEndpoint<FoldersEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetFolders";
    public string Summary { get; } = "Get a paginated list of folders";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<PaginationResult<FolderListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] GetFoldersRequest request,
        [AsParameters] PaginationParameters paginationParameters,
        [AsParameters] SortingParameters sortingParameters,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetFoldersMapper.ToQuery(
                request: request,
                paginationParameters: paginationParameters,
                sortingParameters: sortingParameters),
            cancellationToken);

        return result.ToHttpResult(onSuccess: page => TypedResults.Ok(GetFoldersMapper.ToResponse(page)));
    }
}
