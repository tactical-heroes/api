using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetPersonalDownloadUrl;

internal sealed class GetPersonalFileDownloadUrlEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.PersonalRoute + FilesEndpoints.IdRoute + "/download-url";
    public string Name { get; } = "GetPersonalFileDownloadUrl";
    public string Summary { get; } = "Get personal file download URL";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<GetPersonalFileDownloadUrlResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetPersonalFileDownloadUrlMapper.ToQuery(id),
            cancellationToken);

        response.Headers.CacheControl = "no-store";
        return result.ToHttpResult(onSuccess: download =>
            TypedResults.Ok(GetPersonalFileDownloadUrlMapper.ToResponse(download)));
    }
}
