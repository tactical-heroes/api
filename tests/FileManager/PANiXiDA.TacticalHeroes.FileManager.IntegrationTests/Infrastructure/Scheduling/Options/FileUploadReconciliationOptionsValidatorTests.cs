using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Scheduling.Options;

public sealed class FileUploadReconciliationOptionsValidatorTests
{
    [Theory(DisplayName = "Reconciliation options should reject settings when enabled configuration is invalid")]
    [InlineData("cron")]
    [InlineData("age")]
    [InlineData("timeout")]
    [InlineData("large-timeout")]
    [InlineData("batch")]
    [InlineData("large-batch")]
    public void Validate_Should_RejectInvalidSettings_When_EnabledConfigurationIsInvalid(string invalidPart)
    {
        var options = new FileUploadReconciliationOptions
        {
            Enabled = true,
            CronSchedule = invalidPart == "cron" ? "invalid" : "0 */5 * * * ?",
            MinimumAge = invalidPart == "age" ? TimeSpan.Zero : TimeSpan.FromMinutes(15),
            StorageRequestTimeout = invalidPart switch
            {
                "timeout" => TimeSpan.Zero,
                "large-timeout" => TimeSpan.MaxValue,
                _ => TimeSpan.FromSeconds(30)
            },
            BatchSize = invalidPart switch
            {
                "batch" => 0,
                "large-batch" => 1001,
                _ => 100
            }
        };

        var result = new FileUploadReconciliationOptionsValidator().Validate(null, options);

        result.Failed.ShouldBeTrue();
    }

    [Theory(DisplayName = "Reconciliation options should accept defaults when enabled state varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_AcceptDefaults_When_EnabledStateVaries(bool enabled)
    {
        var options = new FileUploadReconciliationOptions { Enabled = enabled };

        var result = new FileUploadReconciliationOptionsValidator().Validate(null, options);

        result.Succeeded.ShouldBeTrue();
    }
}
