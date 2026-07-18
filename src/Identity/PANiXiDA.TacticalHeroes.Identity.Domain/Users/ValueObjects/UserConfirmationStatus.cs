namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

public sealed class UserConfirmationStatus : ValueObject
{
    private UserConfirmationStatus(bool isConfirmed)
    {
        IsConfirmed = isConfirmed;
    }

    public bool IsConfirmed { get; }

    public static UserConfirmationStatus Unconfirmed()
    {
        return new UserConfirmationStatus(isConfirmed: false);
    }

    public static UserConfirmationStatus Confirmed()
    {
        return new UserConfirmationStatus(isConfirmed: true);
    }

    public static UserConfirmationStatus From(bool isConfirmed)
    {
        return new UserConfirmationStatus(isConfirmed: isConfirmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IsConfirmed;
    }
}
