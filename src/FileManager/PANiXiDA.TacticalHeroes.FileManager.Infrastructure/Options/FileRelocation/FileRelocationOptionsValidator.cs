using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Options.FileRelocation;

public sealed class FileRelocationOptionsValidator : IValidateOptions<FileRelocationOptions>
{
    public ValidateOptionsResult Validate(string? name, FileRelocationOptions options)
    {
        return options.CleanupDelay > TimeSpan.Zero
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("File relocation cleanup delay must be positive.");
    }
}
