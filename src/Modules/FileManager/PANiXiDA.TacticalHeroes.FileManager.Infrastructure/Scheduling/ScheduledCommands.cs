using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Recurring.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Recurring.ReconcileDeletions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling;

public static class ScheduledCommands
{
    private const string SectionName = $"{nameof(FileManager)}:Schedules";

    public static void AddFileManagerSchedules(this WolverineScheduleConfiguration schedules)
    {
        schedules.AddRecurringCommand<ReconcileFileUploadsCommand>(parentSectionName: SectionName);
        schedules.AddRecurringCommand<ReconcileFolderDeletionsCommand>(parentSectionName: SectionName);
    }
}
