using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

internal sealed class UpdateUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = UsersEndpoints.IdRoute;
    public string Name { get; } = "UpdateUser";
    public string Summary { get; } = "Update user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPut(builder.Route, HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateUserRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var isCurrentUser = Guid.TryParse(user.GetClaim(OpenIddictConstants.Claims.Subject), out var currentUserId)
            && currentUserId == id;
        var cookie = isCurrentUser
            ? await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme)
            : null;
        var result = await mediator.SendAsync(
            UpdateUserMapper.ToCommand(request: request, id: id),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        if (isCurrentUser
            && cookie?.Succeeded == true
            && cookie.Principal?.GetClaim(OpenIddictConstants.Claims.Subject) == id.ToString())
        {
            var principal = await mediator.QueryAsync(UpdateUserMapper.ToSessionQuery(id), cancellationToken);
            if (principal.IsSuccess)
            {
                await httpContext.SignInAsync(
                    scheme: IdentityConstants.ApplicationScheme,
                    principal: AuthenticatedUserPrincipalFactory.Create(
                        new AuthenticatedUserReadModel(
                            Id: id,
                            Email: request.Email,
                            UserName: request.UserName,
                            Claims: principal.Value.Claims)),
                    properties: cookie.Properties);
            }
        }

        return TypedResults.NoContent();
    }
}
