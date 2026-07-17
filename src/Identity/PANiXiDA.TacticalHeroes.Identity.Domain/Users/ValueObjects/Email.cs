using System.Net.Mail;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

public sealed class Email : ValueObject
{
    public const int MaxLength = 320;

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(
                error: Error.Validation(message: "Email cannot be empty.")
                    .WithField(nameof(Email)));
        }

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Length > MaxLength)
        {
            return Result.Failure<Email>(
                error: Error.Validation(message: $"Email cannot be longer than {MaxLength} characters.")
                    .WithField(nameof(Email)));
        }

        try
        {
            var mailAddress = new MailAddress(address: normalizedValue);

            if (!string.Equals(mailAddress.Address, normalizedValue, StringComparison.Ordinal))
            {
                return Result.Failure<Email>(
                    error: Error.Validation(message: "Email has invalid format.")
                        .WithField(nameof(Email)));
            }
        }
        catch (FormatException)
        {
            return Result.Failure<Email>(
                error: Error.Validation(message: "Email has invalid format.")
                    .WithField(nameof(Email)));
        }

        return Result.Success(value: new Email(value: normalizedValue));
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
