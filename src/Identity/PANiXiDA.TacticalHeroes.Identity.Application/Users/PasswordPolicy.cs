namespace PANiXiDA.TacticalHeroes.Identity.Application.Users;

public static class PasswordPolicy
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 128;

    public static Result<ValidatedPassword> Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Failure<ValidatedPassword>(
                Error.Validation("Password cannot be empty.")
                    .WithField(nameof(password)));
        }

        if (password.Length < MinimumLength)
        {
            return Result.Failure<ValidatedPassword>(
                Error.Validation($"Password cannot be shorter than {MinimumLength} characters.")
                    .WithField(nameof(password)));
        }

        if (password.Length > MaximumLength)
        {
            return Result.Failure<ValidatedPassword>(
                Error.Validation($"Password cannot be longer than {MaximumLength} characters.")
                    .WithField(nameof(password)));
        }

        return Result.Success(new ValidatedPassword(password));
    }
}
