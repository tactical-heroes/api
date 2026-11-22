using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Options.FileRelocation;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Options.FileRelocation;

public sealed class FileRelocationOptionsValidator : IValidateOptions<FileRelocationOptions>
{
    public ValidateOptionsResult Validate(string? name, FileRelocationOptions options)
    {
        return options.CleanupDelay > TimeSpan.Zero
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("File relocation cleanup delay must be positive.");
    }
}
