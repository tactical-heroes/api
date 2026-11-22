using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Move;

internal sealed class MoveFolderEndpoint : IEndpoint<FoldersEndpoints>
{
    public string Route { get; } = FoldersEndpoints.IdRoute + "/parent";
    public string Name { get; } = "MoveFolder";
    public string Summary { get; } = "Move folder";

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
        MoveFolderRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            MoveFolderMapper.ToCommand(request: request, id: id), cancellationToken);

        return result.ToHttpResult(onSuccess: () => TypedResults.NoContent());
    }
}
