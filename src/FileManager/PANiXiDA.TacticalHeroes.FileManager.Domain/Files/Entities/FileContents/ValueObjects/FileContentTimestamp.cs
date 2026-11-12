namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;

public sealed partial class FileContentTimestamp : ValueObject
{
    private FileContentTimestamp(DateTimeOffset value)
    {
        Value = value;
    }

    public DateTimeOffset Value { get; }

    public static FileContentTimestamp Create(DateTimeOffset value)
    {
        return new FileContentTimestamp(value.ToUniversalTime());
    }
}
