using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling;

public static class ScheduledCommands
{
    public static void AddFileManagerSchedules(this WolverineScheduleConfiguration schedules)
    {
        schedules.AddRecurringCommand<ReconcilePersonalFileUploadsCommand>(parentSectionName: "FileManager:Schedules");
    }
}
