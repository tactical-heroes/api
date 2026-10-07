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
        return MediaTypeHeaderValue.TryParse(value, out var contentType) &&
            contentType.MediaType is { Length: > 0 and <= MaxLength } mediaType &&
            !mediaType.Contains('*') &&
            contentType.Parameters.Count == 0
                ? Result.Success(new FileContentType(mediaType.ToLowerInvariant()))
                : Result.Failure<FileContentType>(
                    Error.Validation("File content type must be a concrete media type without parameters.")
                        .WithField(nameof(FileContentType)));
    }
}
