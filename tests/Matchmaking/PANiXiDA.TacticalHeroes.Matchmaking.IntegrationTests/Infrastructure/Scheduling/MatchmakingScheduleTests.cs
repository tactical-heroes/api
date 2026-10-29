using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.Configurations;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Scheduling;

public sealed class MatchmakingScheduleTests
{
    [Theory(DisplayName = "Scheduling should respect the enabled flag when enabled state varies")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AddMatchmakingSchedules_Should_RespectEnabledFlag_When_EnabledStateVaries(bool enabled)
    {
        await using var options = new WolverineOptions();
        var schedules = new WolverineScheduleConfiguration(options, CreateConfiguration(enabled));

        schedules.AddMatchmakingSchedules();

        var schedule = options.Schedules.FindByName("matchmaking-process");
        (schedule is not null).ShouldBe(enabled);
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

    private static IConfiguration CreateConfiguration(bool enabled, string cron = "*/5 * * * * *")
    {
        const string Prefix = "Matchmaking:Schedules:ProcessMatchmakingCommand";
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{Prefix}:Enabled"] = enabled.ToString(),
                [$"{Prefix}:Name"] = "matchmaking-process",
                [$"{Prefix}:CronExpression"] = cron,
                [$"{Prefix}:TimeZoneId"] = "UTC"
            })
            .Build();
    }
}
