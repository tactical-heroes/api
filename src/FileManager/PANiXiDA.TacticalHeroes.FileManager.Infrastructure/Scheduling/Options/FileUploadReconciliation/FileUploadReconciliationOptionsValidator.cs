using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

internal sealed class FileUploadReconciliationOptionsValidator
    : IValidateOptions<FileUploadReconciliationOptions>
{
    public ValidateOptionsResult Validate(string? name, FileUploadReconciliationOptions options)
    {
        List<string> failures = [];
        if (options.MinimumAge <= TimeSpan.Zero)
        {
            failures.Add("Upload reconciliation minimum age must be positive.");
        }

        if (options.StorageRequestTimeout <= TimeSpan.Zero
            || options.StorageRequestTimeout.TotalMilliseconds > uint.MaxValue - 1)
        {
            failures.Add("Upload reconciliation storage timeout must be positive and fit a cancellation timer.");
        }

        if (options.BatchSize is < 1 or > 1000)
        {
            failures.Add("Upload reconciliation batch size must be between 1 and 1000.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
