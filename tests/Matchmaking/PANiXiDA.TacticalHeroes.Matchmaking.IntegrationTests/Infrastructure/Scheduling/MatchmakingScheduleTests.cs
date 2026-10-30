using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Scheduling;

public sealed class MatchmakingScheduleTests
{
    [Theory(DisplayName = "Scheduling should respect the enabled flag when enabled state varies")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task AddMatchmakingSchedules_Should_RespectEnabledFlag_When_EnabledStateVaries(
        bool enabled, bool expirationEnabled)
    {
        await using var options = new WolverineOptions();
        var schedules = new WolverineScheduleConfiguration(
            options, CreateConfiguration(enabled: enabled, expirationEnabled: expirationEnabled));

        schedules.AddMatchmakingSchedules();

        var schedule = options.Schedules.FindByName("matchmaking-process");
        (schedule is not null).ShouldBe(enabled);
        var expirationSchedule = options.Schedules.FindByName("matchmaking-expire-searches");
        (expirationSchedule is not null).ShouldBe(expirationEnabled);
    }

    [Theory(DisplayName = "Scheduling should reject the schedule when enabled cron is unsupported")]
    [InlineData("invalid")]
    [InlineData("* * * * * *")]
    public async Task AddMatchmakingSchedules_Should_RejectSchedule_When_EnabledCronIsUnsupported(string cron)
    {
        await using var options = new WolverineOptions();
        var schedules = new WolverineScheduleConfiguration(options, CreateConfiguration(enabled: true, cron: cron));

        var exception = Record.Exception(() => schedules.AddMatchmakingSchedules());

        exception.ShouldBeOfType<OptionsValidationException>();
    }

    [Theory(DisplayName = "Scheduling should reject expiration schedule when enabled cron is unsupported")]
    [InlineData("invalid")]
    [InlineData("* * * * * *")]
    public async Task AddMatchmakingSchedules_Should_RejectExpirationSchedule_When_EnabledCronIsUnsupported(string cron)
    {
        await using var options = new WolverineOptions();
        var configuration = CreateConfiguration(enabled: false, expirationEnabled: true, expirationCron: cron);
        var schedules = new WolverineScheduleConfiguration(options, configuration);

        var exception = Record.Exception(() => schedules.AddMatchmakingSchedules());

        exception.ShouldBeOfType<OptionsValidationException>();
    }

    private static IConfiguration CreateConfiguration(
        bool enabled,
        string cron = "*/5 * * * * *",
        bool expirationEnabled = false,
        string expirationCron = "*/5 * * * * *")
    {
        const string Prefix = "Matchmaking:Schedules:ProcessMatchmakingCommand";
        const string ExpirationPrefix = "Matchmaking:Schedules:ExpireMatchSearchesCommand";
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{Prefix}:Enabled"] = enabled.ToString(),
                [$"{Prefix}:Name"] = "matchmaking-process",
                [$"{Prefix}:CronExpression"] = cron,
                [$"{Prefix}:TimeZoneId"] = "UTC",
                [$"{ExpirationPrefix}:Enabled"] = expirationEnabled.ToString(),
                [$"{ExpirationPrefix}:Name"] = "matchmaking-expire-searches",
                [$"{ExpirationPrefix}:CronExpression"] = expirationCron,
                [$"{ExpirationPrefix}:TimeZoneId"] = "UTC"
            })
            .Build();
    }
}
