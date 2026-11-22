using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Rename;

internal sealed class RenameFolderEndpoint : IEndpoint<FoldersEndpoints>
{
    public string Route { get; } = FoldersEndpoints.IdRoute + "/name";
    public string Name { get; } = "RenameFolder";
    public string Summary { get; } = "Rename folder";

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
        RenameFolderRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            RenameFolderMapper.ToCommand(request: request, id: id), cancellationToken);

        return result.ToHttpResult(onSuccess: () => TypedResults.NoContent());
    }
}
