using System.Security.Claims;

using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;

internal sealed class ChangePasswordEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/change-password";
    public string Name { get; } = "ChangePassword";
    public string Summary { get; } = "Change current user password";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(pattern: builder.Route, handler: HandleAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirst(@type: OpenIddictConstants.Claims.Subject)?.Value;

        if (!Guid.TryParse(input: userIdValue, result: out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var result = await mediator.SendAsync(
            command: ChangePasswordMapper.ToCommand(request: request, userId: userId),
            cancellationToken: cancellationToken);

        return result.ToHttpResult(onSuccess: TypedResults.NoContent);
    }
}
