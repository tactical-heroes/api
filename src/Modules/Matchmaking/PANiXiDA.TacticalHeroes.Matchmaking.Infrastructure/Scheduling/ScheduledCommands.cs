using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Expire;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Process;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Scheduling;

public static class ScheduledCommands
{
    public static void AddMatchmakingSchedules(this WolverineScheduleConfiguration schedules)
    {
        schedules.AddRecurringCommand<ProcessMatchmakingCommand>(
            parentSectionName: $"{nameof(Matchmaking)}:Schedules");
        schedules.AddRecurringCommand<ExpireMatchSearchesCommand>(
            parentSectionName: $"{nameof(Matchmaking)}:Schedules");
    }
}
