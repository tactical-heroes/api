using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.DependencyInjection;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Constants;

using PANiXiDA.TacticalHeroes.Host.Configurations.Messaging;

using PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Scheduling;
using PANiXiDA.TacticalHeroes.Modules.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Scheduling;
using PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Persistence.Core;

using BattleApplicationAssembly = PANiXiDA.TacticalHeroes.Modules.Battle.Application.ApplicationAssembly;
using BuildsApplicationAssembly = PANiXiDA.TacticalHeroes.Modules.Builds.Application.ApplicationAssembly;
using CompendiumApplicationAssembly = PANiXiDA.TacticalHeroes.Modules.Compendium.Application.ApplicationAssembly;
using FileManagerApplicationAssembly = PANiXiDA.TacticalHeroes.Modules.FileManager.Application.ApplicationAssembly;
using GamesApplicationAssembly = PANiXiDA.TacticalHeroes.Modules.Games.Application.ApplicationAssembly;
using IdentityApplicationAssembly = PANiXiDA.TacticalHeroes.Modules.Identity.Application.ApplicationAssembly;
using MatchmakingApplicationAssembly = PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.ApplicationAssembly;
using PlayersApplicationAssembly = PANiXiDA.TacticalHeroes.Modules.Players.Application.ApplicationAssembly;

using Wolverine.Util;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class MessagingConfiguration
{
    static MessagingConfiguration()
    {
        WolverineMessageNaming.InsertFirst<ModuleMessageNaming>();
    }

    internal static WebApplicationBuilder AddMessaging(
        this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(
            EfConstants.PostgreSqlConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

        builder.Host.UseWolverineMediator(
            messageStoreConnectionString: connectionString,
            configuration: builder.Configuration,
            configureModules: modules => modules
                .AddModule<IdentityWriteDbContext>(
                    IdentityApplicationAssembly.Instance,
                    typeof(IdentityWriteDbContext).Assembly)
                .AddModule<CompendiumWriteDbContext>(
                    CompendiumApplicationAssembly.Instance,
                    typeof(CompendiumWriteDbContext).Assembly)
                .AddModule<FileManagerWriteDbContext>(
                    FileManagerApplicationAssembly.Instance,
                    typeof(FileManagerWriteDbContext).Assembly)
                .AddModule<BattleWriteDbContext>(
                    BattleApplicationAssembly.Instance,
                    typeof(BattleWriteDbContext).Assembly)
                .AddModule<MatchmakingWriteDbContext>(
                    MatchmakingApplicationAssembly.Instance,
                    typeof(MatchmakingWriteDbContext).Assembly)
                .AddModule<GamesWriteDbContext>(
                    GamesApplicationAssembly.Instance,
                    typeof(GamesWriteDbContext).Assembly)
                .AddModule<BuildsWriteDbContext>(
                    BuildsApplicationAssembly.Instance,
                    typeof(BuildsWriteDbContext).Assembly)
                .AddModule<PlayersWriteDbContext>(
                    PlayersApplicationAssembly.Instance,
                    typeof(PlayersWriteDbContext).Assembly),
            configureKafka: null,
            configureRequestBehaviors: null,
            configureSchedules: schedules =>
            {
                schedules.AddFileManagerSchedules();
                schedules.AddMatchmakingSchedules();
            });

        return builder;
    }
}
