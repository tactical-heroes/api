using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetPersonalDetails;

internal sealed class GetPersonalFileDetailsEndpoint : IEndpoint<FilesEndpoints>
{
    public string Route { get; } = FilesEndpoints.PersonalRoute + FilesEndpoints.IdRoute;
    public string Name { get; } = "GetPersonalFileDetails";
    public string Summary { get; } = "Get personal file";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces<GetPersonalFileDetailsResponse>(StatusCodes.Status200OK)
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
            GetPersonalFileDetailsMapper.ToQuery(id: id),
            cancellationToken);

        return result.ToHttpResult(onSuccess: file =>
            TypedResults.Ok(value: GetPersonalFileDetailsMapper.ToResponse(file: file)));
    }
}
