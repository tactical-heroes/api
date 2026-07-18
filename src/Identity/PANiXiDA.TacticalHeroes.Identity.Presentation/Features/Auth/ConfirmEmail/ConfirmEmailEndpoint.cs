using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ConfirmEmail;

internal sealed class ConfirmEmailEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/confirm-email";
    public string Name { get; } = "ConfirmEmail";
    public string Summary { get; } = "Confirm email";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        ConfirmEmailRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            ConfirmEmailMapper.ToCommand(request: request),
            cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
