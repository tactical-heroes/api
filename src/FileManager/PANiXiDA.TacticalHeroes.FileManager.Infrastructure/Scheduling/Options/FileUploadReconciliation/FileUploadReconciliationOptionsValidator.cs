using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

public sealed class FileUploadReconciliationOptionsValidator
    : IValidateOptions<FileUploadReconciliationOptions>
{
    public ValidateOptionsResult Validate(string? name, FileUploadReconciliationOptions options)
    {
        List<string> failures = [];
        if (options.MinimumAge <= TimeSpan.Zero)
        {
            failures.Add("Upload reconciliation minimum age must be positive.");
        }

        if (options.BatchSize is < 1 or > 1000)
        {
            failures.Add("Upload reconciliation batch size must be between 1 and 1000.");
        }

        if (options.ExpirationAge <= options.MinimumAge)
        {
            failures.Add("Upload expiration age must be greater than the reconciliation minimum age.");
        }

        if (options.DeletedFileRecheckWindow <= TimeSpan.Zero)
        {
            failures.Add("Deleted file recheck window must be positive.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
