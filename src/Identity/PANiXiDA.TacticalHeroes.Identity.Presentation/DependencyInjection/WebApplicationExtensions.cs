using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication UsePresentation(
        this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttp(Assembly.GetExecutingAssembly());

        return app;
    }
}
