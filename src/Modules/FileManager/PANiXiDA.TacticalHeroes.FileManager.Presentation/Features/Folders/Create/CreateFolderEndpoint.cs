using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Create;

internal sealed class CreateFolderEndpoint : IEndpoint<FoldersEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateFolder";
    public string Summary { get; } = "Create folder";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .Produces<CreateFolderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        CreateFolderRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            CreateFolderMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreateFolderMapper.ToResponse(id: id),
                routeName: new GetFolderDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
