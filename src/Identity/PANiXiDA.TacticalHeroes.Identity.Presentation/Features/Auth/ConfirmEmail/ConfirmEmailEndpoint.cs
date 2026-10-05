using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ConfirmEmail;

internal sealed class ConfirmEmailEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/confirm-email";
    public string Name { get; } = "ConfirmEmail";
    public string Summary { get; } = "Confirm email";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(pattern: builder.Route, handler: HandleAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        ConfirmEmailRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: ConfirmEmailMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
