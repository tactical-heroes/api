using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreateUploadUrl;

internal sealed class CreateFileUploadUrlEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = "/upload-url";
    public string Name { get; } = "CreateFileUploadUrl";
    public string Summary { get; } = "Create file upload URL";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .Produces<CreateFileUploadUrlResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        CreateFileUploadUrlRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(CreateFileUploadUrlMapper.ToCommand(request), cancellationToken);

        return result.ToHttpResult(onSuccess: upload =>
            TypedResults.CreatedAtRoute(
                value: CreateFileUploadUrlMapper.ToResponse(upload),
                routeName: new GetFileDetailsEndpoint().Name,
                routeValues: new { id = upload.Id }));
    }
}
