using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Scheduling.Reconciliation;

public sealed class FolderDeletionScheduleTests
{
    [Theory(DisplayName = "FileManager scheduling should respect the enabled flag when enabled state varies")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AddFileManagerSchedules_Should_RespectEnabledFlag_When_EnabledStateVaries(bool enabled)
    {
        await using var options = new WolverineOptions();
        var schedules = new WolverineScheduleConfiguration(options, CreateConfiguration(enabled));

        schedules.AddFileManagerSchedules();

        var schedule = options.Schedules.FindByName("file-manager-reconcile-folder-deletions");
        (schedule is not null).ShouldBe(enabled);
    }

    [Fact(DisplayName = "FileManager scheduling should reject the schedule when enabled cron is invalid")]
    public async Task AddFileManagerSchedules_Should_RejectSchedule_When_EnabledCronIsInvalid()
    {
        await using var options = new WolverineOptions();
        var schedules = new WolverineScheduleConfiguration(
            options,
            CreateConfiguration(enabled: true, cron: "invalid"));

        var exception = Record.Exception(() => schedules.AddFileManagerSchedules());

        exception.ShouldBeOfType<OptionsValidationException>();
    }

    private static IConfiguration CreateConfiguration(bool enabled, string cron = "*/5 * * * *")
    {
        const string Prefix = "FileManager:Schedules:ReconcileFolderDeletionsCommand";
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileManager:Schedules:ReconcileFileUploadsCommand:Enabled"] = "false",
                [$"{Prefix}:Enabled"] = enabled.ToString(),
                [$"{Prefix}:Name"] = "file-manager-reconcile-folder-deletions",
                [$"{Prefix}:CronExpression"] = cron,
                [$"{Prefix}:TimeZoneId"] = "UTC"
            })
            .Build();
    }
}
