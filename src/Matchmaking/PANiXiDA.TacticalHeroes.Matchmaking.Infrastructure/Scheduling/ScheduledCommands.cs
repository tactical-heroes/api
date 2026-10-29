using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling;

public static class ScheduledCommands
{
    public static void AddMatchmakingSchedules(this WolverineScheduleConfiguration schedules)
    {
        schedules.AddRecurringCommand<ProcessMatchmakingCommand>(
            parentSectionName: $"{nameof(Matchmaking)}:Schedules");
    }
}
