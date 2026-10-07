using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;

public sealed partial class FileStatus : Enumeration<FileStatus>
{
    public const int MaxLength = 32;

    public static readonly FileStatus PendingUpload = new(1, nameof(PendingUpload));
    public static readonly FileStatus Ready = new(2, nameof(Ready));
    public static readonly FileStatus Deleting = new(3, nameof(Deleting));
    public static readonly FileStatus Deleted = new(4, nameof(Deleted));

    private FileStatus(int id, string name)
        : base(id, name)
    {
    }

    public bool IsDeletingOrDeleted => this == Deleting || this == Deleted;

    public static Result<FileStatus> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FileStatus>(
                Error.Validation("File status is required.")
                    .WithField(nameof(FileStatus)));
        }

        var normalizedValue = value.Trim();

        return TryFromName(normalizedValue, out var result) && result is not null
            ? Result.Success(result)
            : Result.Failure<FileStatus>(
                Error.Validation($"File status '{normalizedValue}' is invalid.")
                    .WithField(nameof(FileStatus)));
    }
}
