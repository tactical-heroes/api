using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetList;

internal sealed class GetFilesEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "GetFiles";
    public string Summary { get; } = "Get file metadata";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<PaginationResult<FileListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] GetFilesRequest request,
        [AsParameters] PaginationParameters paginationParameters,
        [AsParameters] SortingParameters sortingParameters,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetFilesMapper.ToQuery(
                request: request,
                paginationParameters: paginationParameters,
                sortingParameters: sortingParameters),
            cancellationToken);

        return result.ToHttpResult(onSuccess: page => TypedResults.Ok(GetFilesMapper.ToResponse(page)));
    }
}
