using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Scheduling.Reconciliation;

public sealed class PersonalFileUploadScheduleTests
{
    [Theory(DisplayName = "Core scheduling should respect the enabled flag when enabled state varies")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AddRecurringCommand_Should_RespectEnabledFlag_When_EnabledStateVaries(bool enabled)
    {
        await using var options = new WolverineOptions();
        var schedules = new WolverineScheduleConfiguration(options, CreateConfiguration(enabled));

        schedules.AddRecurringCommand<ReconcilePersonalFileUploadsCommand>(parentSectionName: "FileManager:Schedules");

        var schedule = options.Schedules.FindByName("file-manager-reconcile-personal-uploads");
        (schedule is not null).ShouldBe(enabled);
    }

    [Fact(DisplayName = "Core scheduling should reject the schedule when enabled cron is invalid")]
    public async Task AddRecurringCommand_Should_RejectSchedule_When_EnabledCronIsInvalid()
    {
        await using var options = new WolverineOptions();
        var schedules = new WolverineScheduleConfiguration(
            options,
            CreateConfiguration(enabled: true, cron: "invalid"));

        var exception = Record.Exception(() => schedules.AddRecurringCommand<ReconcilePersonalFileUploadsCommand>(
            parentSectionName: "FileManager:Schedules"));

        exception.ShouldBeOfType<OptionsValidationException>();
    }

    private static IConfiguration CreateConfiguration(bool enabled, string cron = "*/5 * * * *")
    {
        const string Prefix = "FileManager:Schedules:ReconcilePersonalFileUploadsCommand";
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{Prefix}:Enabled"] = enabled.ToString(),
                [$"{Prefix}:Name"] = "file-manager-reconcile-personal-uploads",
                [$"{Prefix}:CronExpression"] = cron,
                [$"{Prefix}:TimeZoneId"] = "UTC"
            })
            .Build();
    }
}
