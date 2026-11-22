using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Update;

internal sealed class UpdateFileEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.IdRoute;
    public string Name { get; } = "UpdateFile";
    public string Summary { get; } = "Update file";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(builder.Route, HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateFileRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            UpdateFileMapper.ToCommand(request: request, id: id), cancellationToken);

        return result.ToHttpResult(onSuccess: () => TypedResults.NoContent());
    }
}
