using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Move;

internal sealed class MoveFileEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.IdRoute + "/parent";
    public string Name { get; } = "MoveFile";
    public string Summary { get; } = "Move file";

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
        MoveFileRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            MoveFileMapper.ToCommand(request: request, id: id), cancellationToken);

        return result.ToHttpResult(onSuccess: () => TypedResults.NoContent());
    }
}
