using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDetails;

internal sealed class GetFileDetailsEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.IdRoute;
    public string Name { get; } = "GetFileDetails";
    public string Summary { get; } = "Get file";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<GetFileDetailsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(
            GetFileDetailsMapper.ToQuery(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: file =>
            TypedResults.Ok(value: GetFileDetailsMapper.ToResponse(file: file)));
    }
}
