using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common.Urls;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Authorize;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

internal sealed class LoginEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/login";
    public string Name { get; } = "Login";
    public string Summary { get; } = "Log in user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .AllowAnonymous()
            .Produces(statusCode: StatusCodes.Status302Found)
            .ProducesValidationProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handle(
        LoginRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var returnUrlValidationResult = AllowedRedirectUrlValidator.Validate(
            url: request.ReturnUrl,
            httpContext: httpContext,
            allowedPath: GetAuthorizePath(),
            fieldName: nameof(LoginRequest.ReturnUrl));

        if (returnUrlValidationResult.IsFailure)
        {
            return returnUrlValidationResult.ToHttpProblem();
        }

        var result = await mediator.SendAsync(
            command: LoginMapper.ToCommand(request: request),
            cancellationToken: cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        await httpContext.SignInAsync(
            scheme: IdentityConstants.ApplicationScheme,
            principal: LoginMapper.ToClaimsPrincipal(user: result.Value));

        return TypedResults.Redirect(url: request.ReturnUrl);
    }

    private static string GetAuthorizePath()
    {
        return string.Concat(
            str0: "/",
            str1: new OAuthEndpoints().Route.TrimEnd(trimChar: '/'),
            str2: new AuthorizeEndpoint().Route);
    }
}
