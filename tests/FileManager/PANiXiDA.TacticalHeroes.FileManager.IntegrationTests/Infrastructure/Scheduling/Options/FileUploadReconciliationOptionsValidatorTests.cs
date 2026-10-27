using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Scheduling.Options;

public sealed class FileUploadReconciliationOptionsValidatorTests
{
    [Theory(DisplayName = "Reconciliation options should reject settings when configuration is invalid")]
    [InlineData("age")]
    [InlineData("batch")]
    [InlineData("large-batch")]
    [InlineData("expiration")]
    [InlineData("short-expiration")]
    [InlineData("equal-expiration")]
    public void Validate_Should_RejectInvalidSettings_When_ConfigurationIsInvalid(string invalidPart)
    {
        var options = new FileUploadReconciliationOptions
        {
            MinimumAge = invalidPart == "age" ? TimeSpan.Zero : TimeSpan.FromMinutes(15),
            ExpirationAge = invalidPart switch
            {
                "expiration" => TimeSpan.Zero,
                "short-expiration" => TimeSpan.FromMinutes(10),
                "equal-expiration" => TimeSpan.FromMinutes(15),
                _ => TimeSpan.FromDays(1)
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

    [Fact(DisplayName = "Reconciliation options should accept defaults when configuration uses defaults")]
    public void Validate_Should_AcceptDefaults_When_ConfigurationUsesDefaults()
    {
        var options = new FileUploadReconciliationOptions();

        var result = new FileUploadReconciliationOptionsValidator().Validate(null, options);

        result.Succeeded.ShouldBeTrue();
    }
}
