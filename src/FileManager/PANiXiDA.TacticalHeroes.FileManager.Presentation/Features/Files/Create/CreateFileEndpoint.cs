using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Create;

internal sealed class CreateFileEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateFile";
    public string Summary { get; } = "Create file";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .DisableAntiforgery()
            .Produces<CreateFileResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] CreateFileRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await using var content = request.File.OpenReadStream();
        var result = await mediator.SendAsync(
            CreateFileMapper.ToCommand(request: request, content: content), cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateFileMapper.ToResponse(id),
                routeName: new GetFileDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
