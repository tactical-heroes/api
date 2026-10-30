using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonal;

internal sealed class CreatePersonalFileEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.PersonalRoute;
    public string Name { get; } = "CreatePersonalFile";
    public string Summary { get; } = "Create personal file";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .DisableAntiforgery()
            .Produces<CreatePersonalFileResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
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
            TypedResults.CreatedAtRoute(
                value: CreatePersonalFileMapper.ToResponse(id: id),
                routeName: new GetFileDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
