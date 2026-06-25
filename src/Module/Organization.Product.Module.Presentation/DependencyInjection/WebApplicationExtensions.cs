using Microsoft.AspNetCore.Builder;

using System.Reflection;

namespace Organization.Product.Module.Presentation.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication UsePresentation(
        this WebApplication app)
    {
        app.UseHttp(Assembly.GetExecutingAssembly());

        return app;
    }
}
