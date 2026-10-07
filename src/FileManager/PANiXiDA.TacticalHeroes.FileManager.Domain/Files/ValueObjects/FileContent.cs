namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileContent : ValueObject
{
    private FileContent(
        FileContentType contentType,
        long size,
        FileChecksum sha256)
    {
        ContentType = contentType;
        Size = size;
        Sha256 = sha256;
    }

    public FileContentType ContentType { get; }
    public long Size { get; }
    public FileChecksum Sha256 { get; }

    public static Result<FileContent> Create(
        string contentType,
        long size,
        string sha256)
    {
        var contentTypeResult = FileContentType.Create(contentType);
        var sizeResult = size > 0
            ? Result.Success()
            : Result.Failure(
                Error.Validation("File size must be greater than zero.")
                    .WithField(nameof(Size)));
        var checksumResult = FileChecksum.Create(sha256);
        var validationResult = Result.Combine(
            contentTypeResult,
            sizeResult,
            checksumResult);

        return validationResult.IsFailure
            ? Result.Failure<FileContent>(validationResult.Errors)
            : Result.Success(new FileContent(
                contentType: contentTypeResult.Value,
                size: size,
                sha256: checksumResult.Value));
    }
}
