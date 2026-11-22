using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Delete;

internal sealed class DeleteFileEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.IdRoute;
    public string Name { get; } = "DeleteFile";
    public string Summary { get; } = "Request file deletion";

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
        var result = await mediator.SendAsync(DeleteFileMapper.ToCommand(id), cancellationToken);

        return result.ToHttpResult(
            onSuccess: () => TypedResults.StatusCode(statusCode: StatusCodes.Status202Accepted));
    }
}
