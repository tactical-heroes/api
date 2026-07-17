using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;
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
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        var account = result.Value;
        var claims = new List<System.Security.Claims.Claim>
        {
            new(OpenIddictConstants.Claims.Subject, account.Id.ToString()),
            new(OpenIddictConstants.Claims.Name, account.UserName),
            new(OpenIddictConstants.Claims.Email, account.Email)
        };
        claims.AddRange(
            account.Claims.Where(claim =>
                claim.Type != OpenIddictConstants.Claims.Subject &&
                claim.Type != OpenIddictConstants.Claims.Name &&
                claim.Type != OpenIddictConstants.Claims.Email));

        var identity = new ClaimsIdentity(
            claims,
            IdentityConstants.ApplicationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        await httpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            new ClaimsPrincipal(identity));

        return TypedResults.Redirect(request.ReturnUrl);
    }
}
