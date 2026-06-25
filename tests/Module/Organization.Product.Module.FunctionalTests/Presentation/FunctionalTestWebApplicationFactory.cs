using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Wolverine;

namespace Organization.Product.Module.FunctionalTests.Presentation;

internal sealed class FunctionalTestWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => services.RunWolverineInSoloMode());
    }
}
