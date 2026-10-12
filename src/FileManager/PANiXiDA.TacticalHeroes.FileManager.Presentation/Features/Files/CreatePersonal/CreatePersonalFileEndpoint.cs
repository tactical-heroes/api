using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonal;

internal sealed class CreatePersonalFileEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = "/personal";
    public string Name { get; } = "CreatePersonalFile";
    public string Summary { get; } = "Create personal file";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .DisableAntiforgery()
            .Produces<CreatePersonalFileResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] CreatePersonalFileRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await using var content = request.File.OpenReadStream();
        var result = await mediator.SendAsync(
            CreatePersonalFileMapper.ToCommand(
                request: request,
                content: content),
            cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.Json(
                data: CreatePersonalFileMapper.ToResponse(id),
                statusCode: StatusCodes.Status201Created));
    }
}
