using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;

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
        [FromForm] CreatePersonalFileRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var subject = user.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!Guid.TryParse(subject, out var userId) || userId == Guid.Empty)
        {
            return TypedResults.Unauthorized();
        }

        if (request.File is null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.File)] = ["File is required."]
            });
        }

        await using var content = request.File.OpenReadStream();
        var result = await mediator.SendAsync(
            CreatePersonalFileMapper.ToCommand(
                request: request,
                content: content,
                userId: userId),
            cancellationToken);

        return result.ToHttpResult(onSuccess: id =>
            TypedResults.Json(
                data: CreatePersonalFileMapper.ToResponse(id),
                statusCode: StatusCodes.Status201Created));
    }
}
