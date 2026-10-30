using PANiXiDA.Core.Domain.AggregateRoots;
using PANiXiDA.Core.Domain.DomainEvents;
using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.DependencyInjection;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Constants;

using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

using Wolverine.EntityFrameworkCore;

using BattleApplicationAssembly = PANiXiDA.TacticalHeroes.Battle.Application.ApplicationAssembly;
using BuildsApplicationAssembly = PANiXiDA.TacticalHeroes.Builds.Application.ApplicationAssembly;
using CompendiumApplicationAssembly = PANiXiDA.TacticalHeroes.Compendium.Application.ApplicationAssembly;
using FileManagerApplicationAssembly = PANiXiDA.TacticalHeroes.FileManager.Application.ApplicationAssembly;
using GamesApplicationAssembly = PANiXiDA.TacticalHeroes.Games.Application.ApplicationAssembly;
using IdentityApplicationAssembly = PANiXiDA.TacticalHeroes.Identity.Application.ApplicationAssembly;
using MatchmakingApplicationAssembly = PANiXiDA.TacticalHeroes.Matchmaking.Application.ApplicationAssembly;
using PlayersApplicationAssembly = PANiXiDA.TacticalHeroes.Players.Application.ApplicationAssembly;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class MessagingConfiguration
{
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

        builder.Services.AddSingleton<IDomainEventScraper>(
            new DomainEventScraper<IAggregateRoot, DomainEvent>(aggregate =>
            {
                var events = aggregate.GetDomainEvents().ToArray();
                aggregate.ClearDomainEvents();
                return events;
            }));

        return builder;
    }
}
