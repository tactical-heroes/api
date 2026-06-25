using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Enumerations;
using Organization.Product.Module.Domain.Users.Events;

namespace Organization.Product.Module.UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact(DisplayName = "Create should create user with normalized values when input is valid")]
    public void Create_Should_Create_User_With_Normalized_Values_When_Input_Is_Valid()
    {
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30);

        var result = User.Create(
            role: " Admin ",
            name: "  John Doe  ",
            email: "  JOHN.DOE@EXAMPLE.COM  ",
            phone: "  +1 (234) 567-8901  ",
            birthDate: birthDate,
            avatar: "  https://example.com/avatar.png  ");

        result.IsSuccess.ShouldBeTrue();

        var user = result.Value;
        user.Id.Value.ShouldNotBe(Guid.Empty);
        user.Role.ShouldBe(UserRole.Admin);
        user.Name.Value.ShouldBe("John Doe");
        user.Email.Value.ShouldBe("john.doe@example.com");
        user.Phone.Value.ShouldBe("+12345678901");
        user.BirthDate.Value.ShouldBe(birthDate);
        user.Avatar.ShouldNotBeNull();
        user.Avatar.Value.ShouldBe("https://example.com/avatar.png");
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Create should return all validation errors when input is invalid")]
    public void Create_Should_Return_All_Validation_Errors_When_Input_Is_Invalid()
    {
        var result = User.Create(
            role: "",
            name: "",
            email: "",
            phone: "",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            avatar: new string('a', 2049));

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(6);
        result.Errors.ShouldContain(error => error.Message == "User role cannot be empty.");
        result.Errors.ShouldContain(error => error.Message == "User name cannot be empty.");
        result.Errors.ShouldContain(error => error.Message == "Email cannot be empty.");
        result.Errors.ShouldContain(error => error.Message == "Phone number cannot be empty.");
        result.Errors.ShouldContain(error => error.Message == "Birth date cannot be in the future.");
        result.Errors.ShouldContain(error => error.Message == "Avatar cannot be longer than 2048 characters.");
    }

    [Fact(DisplayName = "Update should update user and raise email changed event when email changes")]
    public void Update_Should_Update_User_And_Raise_Email_Changed_Event_When_Email_Changes()
    {
        var user = User.Create(
            role: "User",
            name: "John Doe",
            email: "old@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: null).Value;
        var newBirthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-35);

        var result = user.Update(
            role: "Admin",
            name: "Jane Doe",
            email: "new@example.com",
            phone: "+19876543210",
            birthDate: newBirthDate,
            avatar: "https://example.com/new-avatar.png");

        result.IsSuccess.ShouldBeTrue();
        user.Role.ShouldBe(UserRole.Admin);
        user.Name.Value.ShouldBe("Jane Doe");
        user.Email.Value.ShouldBe("new@example.com");
        user.Phone.Value.ShouldBe("+19876543210");
        user.BirthDate.Value.ShouldBe(newBirthDate);
        user.Avatar.ShouldNotBeNull();
        user.Avatar.Value.ShouldBe("https://example.com/new-avatar.png");

        var domainEvent = user.GetDomainEvents().ShouldHaveSingleItem();
        var emailChanged = domainEvent.ShouldBeOfType<UserEmailChanged>();
        emailChanged.UserId.ShouldBe(user.Id.Value);
        emailChanged.OldEmail.ShouldBe("old@example.com");
        emailChanged.NewEmail.ShouldBe("new@example.com");
    }

    [Fact(DisplayName = "Update should not raise email changed event when email is not changed")]
    public void Update_Should_Not_Raise_Email_Changed_Event_When_Email_Is_Not_Changed()
    {
        var user = User.Create(
            role: "User",
            name: "John Doe",
            email: "same@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;

        var result = user.Update(
            role: "Moderator",
            name: "Jane Doe",
            email: "same@example.com",
            phone: "+19876543210",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-40),
            avatar: null);

        result.IsSuccess.ShouldBeTrue();
        user.Email.Value.ShouldBe("same@example.com");
        user.Avatar.ShouldBeNull();
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Update should return failure and keep current state when input is invalid")]
    public void Update_Should_Return_Failure_And_Keep_Current_State_When_Input_Is_Invalid()
    {
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30);
        var user = User.Create(
            role: "User",
            name: "John Doe",
            email: "john@example.com",
            phone: "+12345678901",
            birthDate: birthDate,
            avatar: "https://example.com/avatar.png").Value;

        var result = user.Update(
            role: "",
            name: "",
            email: "invalid-email",
            phone: "123",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-17),
            avatar: new string('a', 2049));

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(6);
        user.Role.ShouldBe(UserRole.User);
        user.Name.Value.ShouldBe("John Doe");
        user.Email.Value.ShouldBe("john@example.com");
        user.Phone.Value.ShouldBe("+12345678901");
        user.BirthDate.Value.ShouldBe(birthDate);
        user.Avatar.ShouldNotBeNull();
        user.Avatar.Value.ShouldBe("https://example.com/avatar.png");
        user.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "ClearDomainEvents should remove collected domain events when events exist")]
    public void ClearDomainEvents_Should_Remove_Collected_Domain_Events_When_Events_Exist()
    {
        var user = User.Create(
            role: "User",
            name: "John Doe",
            email: "old@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var updateResult = user.Update(
            role: "User",
            name: "John Doe",
            email: "new@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png");

        updateResult.IsSuccess.ShouldBeTrue();
        user.GetDomainEvents().ShouldNotBeEmpty();

        user.ClearDomainEvents();

        user.GetDomainEvents().ShouldBeEmpty();
    }
}
