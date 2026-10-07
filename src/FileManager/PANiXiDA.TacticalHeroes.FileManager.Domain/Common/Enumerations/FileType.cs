using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

public sealed partial class FileType : Enumeration<FileType>
{
    public const int MaxLength = 32;

    public static readonly FileType Avatar = new(1, nameof(Avatar));

    private FileType(int id, string name)
        : base(id, name)
    {
    }

    public static Result<FileType> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FileType>(
                Error.Validation("File type is required.")
                    .WithField(nameof(FileType)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var result) && result is not null
            ? Result.Success(result)
            : Result.Failure<FileType>(
                Error.Validation($"File type '{normalizedValue}' is invalid.")
                    .WithField(nameof(FileType)));
    }
}
