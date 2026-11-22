namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

public readonly partial record struct UserId : IStronglyTypedId
{
    private UserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static Result<UserId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<UserId>(
                error: Error.Validation(message: "User id cannot be empty."))
            : Result.Success(value: new UserId(value: value));
    }

    public static Result<UserId?> Create(Guid? value)
    {
        return value is null
            ? Result.Success<UserId?>(value: null)
            : Create(value.Value).Map(id => (UserId?)id);
    }
}
