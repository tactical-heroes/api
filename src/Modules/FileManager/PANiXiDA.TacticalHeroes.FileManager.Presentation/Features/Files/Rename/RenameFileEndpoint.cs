using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Rename;

internal sealed class RenameFileEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.IdRoute + "/name";
    public string Name { get; } = "RenameFile";
    public string Summary { get; } = "Rename file";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPatch(builder.Route, HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        RenameFileRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            RenameFileMapper.ToCommand(request: request, id: id), cancellationToken);

        return result.ToHttpResult(onSuccess: () => TypedResults.NoContent());
    }
}
