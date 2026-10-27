namespace PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;

public sealed partial class BuildName : ValueObject
{
    public const int MaxLength = 128;

    private BuildName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<BuildName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<BuildName>(
                error: Error.Validation(message: "Build name cannot be empty.")
                    .WithField(nameof(BuildName)));
        }

        var normalizedValue = value.Trim();

        return normalizedValue.Length <= MaxLength
            ? Result.Success(value: new BuildName(value: normalizedValue))
            : Result.Failure<BuildName>(
                error: Error
                    .Validation(
                        message: $"Build name cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(BuildName)));
    }
}
