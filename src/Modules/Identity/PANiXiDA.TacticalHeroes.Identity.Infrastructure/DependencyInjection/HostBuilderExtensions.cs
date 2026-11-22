using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseInfrastructure(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.ConfigureWolverine(options =>
            {
                options.CodeGeneration.AlwaysUseServiceLocationFor<UserManager<ApplicationUser>>();
                options.CodeGeneration.AlwaysUseServiceLocationFor<RoleManager<ApplicationRole>>();
                options.CodeGeneration.AlwaysUseServiceLocationFor<IOpenIddictApplicationManager>();
                options.CodeGeneration.AlwaysUseServiceLocationFor<IOpenIddictTokenManager>();
                options.CodeGeneration.AlwaysUseServiceLocationFor<IOpenIddictAuthorizationManager>();
            });
        });

        return hostBuilder;
    }
}
