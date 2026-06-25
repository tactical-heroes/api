namespace Organization.Product.Module.Domain.Users.ValueObjects;

public sealed class BirthDate : ValueObject
{
    public const int MinimumAge = 18;

    private BirthDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static Result<BirthDate> Create(DateOnly value)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (value > today)
        {
            return Result.Failure<BirthDate>(
                Error.Validation("Birth date cannot be in the future.")
                    .WithField(nameof(BirthDate)));
        }

        var birthDate = new BirthDate(value);

        if (!birthDate.IsAtLeast(MinimumAge, today))
        {
            return Result.Failure<BirthDate>(
                Error.Validation($"User must be at least {MinimumAge} years old.")
                    .WithField(nameof(BirthDate)));
        }

        return Result.Success(birthDate);
    }

    public int GetAge(DateOnly onDate)
    {
        if (onDate < Value)
        {
            throw new ArgumentOutOfRangeException(
                nameof(onDate),
                "The calculation date cannot be earlier than birth date.");
        }

        var years = onDate.Year - Value.Year;

        var hadBirthdayThisYear = onDate.Month > Value.Month
            || (onDate.Month == Value.Month && onDate.Day >= Value.Day);

        if (!hadBirthdayThisYear)
        {
            years--;
        }

        return years;
    }

    public bool IsAtLeast(int age, DateOnly onDate)
    {
        return GetAge(onDate) >= age;
    }

    public override string ToString()
    {
        return Value.ToString("yyyy-MM-dd");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
