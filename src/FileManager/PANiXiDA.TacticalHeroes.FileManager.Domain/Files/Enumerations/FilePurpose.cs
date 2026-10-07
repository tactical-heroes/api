using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;

public sealed partial class FilePurpose : Enumeration<FilePurpose>
{
    public const int MaxLength = 32;

    public static readonly FilePurpose Avatar = new(1, nameof(Avatar));

    private FilePurpose(int id, string name)
        : base(id, name)
    {
    }

    public static Result<FilePurpose> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FilePurpose>(
                Error.Validation("File purpose is required.")
                    .WithField(nameof(FilePurpose)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var result) && result is not null
            ? Result.Success(result)
            : Result.Failure<FilePurpose>(
                Error.Validation($"File purpose '{normalizedValue}' is invalid.")
                    .WithField(nameof(FilePurpose)));
    }
}
