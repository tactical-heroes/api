using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.ReconcileDeletions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Scheduling;

public static class ScheduledCommands
{
    private const string SectionName = $"{nameof(FileManager)}:Schedules";

    public static void AddFileManagerSchedules(this WolverineScheduleConfiguration schedules)
    {
        schedules.AddRecurringCommand<ReconcileFileUploadsCommand>(parentSectionName: SectionName);
        schedules.AddRecurringCommand<ReconcileFolderDeletionsCommand>(parentSectionName: SectionName);
    }
}
