using Organization.Product.Module.Domain.Users.Enumerations;
using Organization.Product.Module.Domain.Users.Events;
using Organization.Product.Module.Domain.Users.ValueObjects;

namespace Organization.Product.Module.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    private User(
        UserId id,
        UserRole role,
        UserName name,
        Email email,
        PhoneNumber phone,
        BirthDate birthDate,
        Avatar? avatar)
        : base(id)
    {
        Role = role;
        Name = name;
        Email = email;
        Phone = phone;
        BirthDate = birthDate;
        Avatar = avatar;
    }

    public UserRole Role { get; private set; }
    public UserName Name { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public BirthDate BirthDate { get; private set; }
    public Avatar? Avatar { get; private set; }

    public static Result<User> Create(
        string role,
        string name,
        string email,
        string phone,
        DateOnly birthDate,
        string? avatar)
    {
        var roleResult = UserRole.Create(role);
        var nameResult = UserName.Create(name);
        var emailResult = Email.Create(email);
        var phoneResult = PhoneNumber.Create(phone);
        var birthDateResult = BirthDate.Create(birthDate);
        var avatarResult = Avatar.Create(avatar);

        var validationResult = Result.Combine(
            roleResult,
            nameResult,
            emailResult,
            phoneResult,
            birthDateResult,
            avatarResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<User>(validationResult.Errors);
        }

        var user = new User(
            UserId.New(),
            roleResult.Value,
            nameResult.Value,
            emailResult.Value,
            phoneResult.Value,
            birthDateResult.Value,
            avatarResult.Value);

        return Result.Success(user);
    }

    public Result Update(
        string role,
        string name,
        string email,
        string phone,
        DateOnly birthDate,
        string? avatar)
    {
        var roleResult = UserRole.Create(role);
        var nameResult = UserName.Create(name);
        var emailResult = Email.Create(email);
        var phoneResult = PhoneNumber.Create(phone);
        var birthDateResult = BirthDate.Create(birthDate);
        var avatarResult = Avatar.Create(avatar);

        var validationResult = Result.Combine(
            roleResult,
            nameResult,
            emailResult,
            phoneResult,
            birthDateResult,
            avatarResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors);
        }

        Role = roleResult.Value;
        Name = nameResult.Value;
        Phone = phoneResult.Value;
        BirthDate = birthDateResult.Value;
        Avatar = avatarResult.Value;

        ChangeEmail(emailResult.Value);

        return Result.Success();
    }

    private void ChangeEmail(Email email)
    {
        if (Email == email)
        {
            return;
        }

        var oldEmail = Email;
        Email = email;

        AddDomainEvent(
            new UserEmailChanged(
                Id.Value,
                oldEmail.Value,
                Email.Value));
    }
}
