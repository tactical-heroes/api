using System.Net.Http.Headers;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileContentType : ValueObject
{
    public const int MaxLength = 255;

    private FileContentType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FileContentType> Create(string value)
    {
        if (!MediaTypeHeaderValue.TryParse(value, out var contentType))
        {
            return Result.Failure<FileContentType>(
                Error.Validation("File content type must be a concrete media type without parameters.")
                    .WithField(nameof(FileContentType)));
        }

        if (contentType.MediaType is not { Length: > 0 and <= MaxLength } mediaType ||
            mediaType.Contains('*') ||
            contentType.Parameters.Count != 0)
        {
            return Result.Failure<FileContentType>(
                Error.Validation("File content type must be a concrete media type without parameters.")
                    .WithField(nameof(FileContentType)));
        }

        return Result.Success(new FileContentType(mediaType.ToLowerInvariant()));
    }
}
