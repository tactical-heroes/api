using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Identity.Application;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

using Wolverine;

using System.Reflection;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseInfrastructure(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        var messageStoreConnectionString =
            configuration.GetConnectionString(EfConstants.PostgreSqlConnectionStringName)
            ?? throw new InvalidOperationException(
                message: $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

        hostBuilder
            .UseWolverineMediator<IdentityWriteDbContext>(
                messageStoreConnectionString,
                ApplicationAssembly.Instance,
                Assembly.GetExecutingAssembly())
            .ConfigureServices(services =>
            {
                services.ConfigureWolverine(options =>
                {
                    options.CodeGeneration.AlwaysUseServiceLocationFor<UserManager<ApplicationUser>>();
                    options.CodeGeneration.AlwaysUseServiceLocationFor<RoleManager<ApplicationRole>>();
                    options.CodeGeneration.AlwaysUseServiceLocationFor<IOpenIddictApplicationManager>();
                    options.CodeGeneration.AlwaysUseServiceLocationFor<IOpenIddictTokenManager>();
                });
            });

        return hostBuilder;
    }
}
