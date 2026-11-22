using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonalUploadUrl;

internal sealed class CreatePersonalFileUploadUrlEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.PersonalRoute + "/upload-url";
    public string Name { get; } = "CreatePersonalFileUploadUrl";
    public string Summary { get; } = "Create personal file upload URL";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .Produces<CreatePersonalFileUploadUrlResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        CreatePersonalFileUploadUrlRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(CreatePersonalFileUploadUrlMapper.ToCommand(request), cancellationToken);

        return result.ToHttpResult(onSuccess: upload =>
            TypedResults.CreatedAtRoute(
                value: CreatePersonalFileUploadUrlMapper.ToResponse(upload),
                routeName: new GetFileDetailsEndpoint().Name,
                routeValues: new { id = upload.Id }));
    }
}
