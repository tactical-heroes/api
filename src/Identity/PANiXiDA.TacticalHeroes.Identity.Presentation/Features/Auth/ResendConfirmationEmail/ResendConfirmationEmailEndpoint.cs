using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/resend-confirmation-email";
    public string Name { get; } = "ResendConfirmationEmail";
    public string Summary { get; } = "Resend email confirmation email";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .AllowAnonymous()
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        ResendConfirmationEmailRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            command: ResendConfirmationEmailMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
