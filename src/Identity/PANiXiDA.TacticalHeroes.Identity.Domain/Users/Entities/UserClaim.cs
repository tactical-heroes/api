using PANiXiDA.TacticalHeroes.Identity.Domain.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities;

public sealed class UserClaim : Entity<Guid>
{
    private UserClaim(
        Guid id,
        ClaimType type,
        ClaimValue value)
        : base(id)
    {
        Type = type;
        Value = value;
    }

    public ClaimType Type { get; private set; }
    public ClaimValue Value { get; private set; }

    internal static Result<UserClaim> Create(string type, string value)
    {
        var typeResult = ClaimType.Create(type);
        var valueResult = ClaimValue.Create(value);

        if (typeResult.IsFailure)
        {
            return Result.Failure<UserClaim>(typeResult.Errors);
        }

        if (valueResult.IsFailure)
        {
            return Result.Failure<UserClaim>(valueResult.Errors);
        }

        return Result.Success(new UserClaim(
            Guid.CreateVersion7(),
            typeResult.Value,
            valueResult.Value));
    }
}
