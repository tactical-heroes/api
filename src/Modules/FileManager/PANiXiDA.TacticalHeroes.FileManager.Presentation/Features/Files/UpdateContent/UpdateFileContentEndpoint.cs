using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.UpdateContent;

internal sealed class UpdateFileContentEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.IdRoute + "/content";
    public string Name { get; } = "UpdateFileContent";
    public string Summary { get; } = "Replace file content";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(builder.Route, HandleAsync)
            .DisableAntiforgery()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        [AsParameters] UpdateFileContentRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await using var content = request.File.OpenReadStream();
        var result = await mediator.SendAsync(
            UpdateFileContentMapper.ToCommand(request: request, id: id, content: content), cancellationToken);

        return result.ToHttpResult(onSuccess: () => TypedResults.NoContent());
    }
}
