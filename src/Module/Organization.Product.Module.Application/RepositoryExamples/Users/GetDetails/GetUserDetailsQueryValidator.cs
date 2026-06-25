using Organization.Product.Module.Domain.Users;

namespace Organization.Product.Module.Application.Users.GetDetails;

public sealed class GetUserDetailsQueryValidator : AbstractValidator<GetUserDetailsQuery>
{
    public GetUserDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(UserId.Create);
    }
}
