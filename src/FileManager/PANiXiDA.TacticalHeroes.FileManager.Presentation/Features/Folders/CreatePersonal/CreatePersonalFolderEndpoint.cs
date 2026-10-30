using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.CreatePersonal;

internal sealed class CreatePersonalFolderEndpoint : IEndpoint<FoldersEndpoints>
{
    public string Route { get; } = "/personal";
    public string Name { get; } = "CreatePersonalFolder";
    public string Summary { get; } = "Create personal folder";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .Produces<CreatePersonalFolderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        CreatePersonalFolderRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            CreatePersonalFolderMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.CreatedAtRoute(
                value: CreatePersonalFolderMapper.ToResponse(id: id),
                routeName: new GetFolderDetailsEndpoint().Name,
                routeValues: new { id }));
    }
}
