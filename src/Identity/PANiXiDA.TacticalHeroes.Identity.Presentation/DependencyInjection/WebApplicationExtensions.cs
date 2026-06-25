using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Token;

using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication UsePresentation(
        this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapOpenIddictTokenEndpoint();
        app.UseHttp(Assembly.GetExecutingAssembly());

        return app;
    }
}
