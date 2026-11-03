using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.ReconcileDeletions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling;

public static class ScheduledCommands
{
    public static void AddFileManagerSchedules(this WolverineScheduleConfiguration schedules)
    {
        schedules.AddRecurringCommand<ReconcileFileUploadsCommand>(
            parentSectionName: $"{nameof(FileManager)}:Schedules");
        schedules.AddRecurringCommand<ReconcileFolderDeletionsCommand>(
            parentSectionName: $"{nameof(FileManager)}:Schedules");
    }
}
