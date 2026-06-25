using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Confirm;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Confirm;

internal sealed class ConfirmRegistrationEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/confirm";
    public string Name { get; } = "ConfirmUserRegistration";
    public string Summary { get; } = "Confirm identity user registration";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(HandleAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        ConfirmRegistrationRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new ConfirmRegistrationCommand(request.UserId, request.ConfirmationToken),
            cancellationToken);

        return result.ToHttpResult(() => TypedResults.NoContent());
    }
}
