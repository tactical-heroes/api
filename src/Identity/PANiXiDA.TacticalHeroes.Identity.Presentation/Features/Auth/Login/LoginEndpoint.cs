using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common.Urls;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

internal sealed class LoginEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/login";
    public string Name { get; } = "Login";
    public string Summary { get; } = "Log in user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status302Found)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handle(
        LoginRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var returnUrlValidationResult = AllowedRedirectUrlValidator.Validate(
            request.ReturnUrl,
            httpContext,
            "/connect/authorize",
            nameof(LoginRequest.ReturnUrl));

        if (returnUrlValidationResult.IsFailure)
        {
            return returnUrlValidationResult.ToHttpProblem();
        }

        var result = await mediator.SendAsync(
            LoginMapper.ToCommand(request),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        await httpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            LoginMapper.ToClaimsPrincipal(result.Value));

        return TypedResults.Redirect(request.ReturnUrl);
    }
}
