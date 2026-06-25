namespace Organization.Product.Module.Application.Users.GetDetails;

public sealed record GetUserDetailsQuery(Guid Id)
    : IQuery<Result<UserDetailsReadModel>>;
