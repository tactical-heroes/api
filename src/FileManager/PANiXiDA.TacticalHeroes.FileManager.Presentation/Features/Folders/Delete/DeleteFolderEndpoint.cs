using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Delete;

internal sealed class DeleteFolderEndpoint : IEndpoint<FoldersEndpoints>
{
    public string Route { get; } = FoldersEndpoints.IdRoute;
    public string Name { get; } = "DeleteFolder";
    public string Summary { get; } = "Delete folder and its contents";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapDelete(builder.Route, HandleAsync)
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(DeleteFolderMapper.ToCommand(id), cancellationToken);

        return result.ToHttpResult(onSuccess: () => TypedResults.Accepted(uri: (string?)null));
    }
}
