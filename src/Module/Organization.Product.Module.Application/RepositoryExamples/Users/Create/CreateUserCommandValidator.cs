using Organization.Product.Module.Domain.Users.Enumerations;
using Organization.Product.Module.Domain.Users.ValueObjects;

namespace Organization.Product.Module.Application.Users.Create;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Role)
            .MustBeValidDomainValue(UserRole.Create);

        RuleFor(command => command.Name)
            .MustBeValidDomainValue(UserName.Create);

        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);

        RuleFor(command => command.Phone)
            .MustBeValidDomainValue(PhoneNumber.Create);

        RuleFor(command => command.BirthDate)
            .MustBeValidDomainValue(BirthDate.Create);

        RuleFor(command => command.Avatar)
            .MustBeValidDomainValue(Avatar.Create);
    }
}
