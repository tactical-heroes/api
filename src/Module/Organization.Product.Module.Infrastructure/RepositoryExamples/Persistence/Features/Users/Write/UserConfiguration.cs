using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Enumerations;
using Organization.Product.Module.Domain.Users.ValueObjects;

namespace Organization.Product.Module.Infrastructure.Persistence.Features.Users.Write;

internal sealed class UserConfiguration : AuditableEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnOrder(0)
            .HasConversion(UserIdConverter)
            .ValueGeneratedNever();

        builder.Property(item => item.Role)
            .HasConversion(UserRoleConverter)
            .IsRequired();

        builder.Property(item => item.Name)
            .HasConversion(UserNameConverter)
            .HasMaxLength(UserName.MaxLength)
            .IsRequired();

        builder.Property(item => item.Email)
            .HasConversion(EmailConverter)
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.HasIndex(item => item.Email)
            .IsUnique();

        builder.Property(item => item.Phone)
            .HasConversion(PhoneNumberConverter)
            .HasMaxLength(PhoneNumber.MaxLength)
            .IsRequired();

        builder.HasIndex(item => item.Phone)
            .IsUnique();

        builder.Property(item => item.BirthDate)
            .HasConversion(BirthDateConverter)
            .IsRequired();

        builder.Property(item => item.Avatar)
            .HasConversion(AvatarConverter)
            .HasMaxLength(Avatar.MaxLength)
            .IsRequired(false);
    }

    private static readonly ValueConverter<UserId, Guid> UserIdConverter = new(
        userId => userId.Value,
        value => UserId.Create(value).Value);

    private static readonly ValueConverter<UserRole, string> UserRoleConverter = new(
        role => role.Name,
        value => UserRole.Create(value).Value);

    private static readonly ValueConverter<UserName, string> UserNameConverter = new(
        name => name.Value,
        value => UserName.Create(value).Value);

    private static readonly ValueConverter<Email, string> EmailConverter = new(
        email => email.Value,
        value => Email.Create(value).Value);

    private static readonly ValueConverter<PhoneNumber, string> PhoneNumberConverter = new(
        phone => phone.Value,
        value => PhoneNumber.Create(value).Value);

    private static readonly ValueConverter<BirthDate, DateOnly> BirthDateConverter = new(
        birthDate => birthDate.Value,
        value => BirthDate.Create(value).Value);

    private static readonly ValueConverter<Avatar?, string?> AvatarConverter = new(
        avatar => avatar == null ? null : avatar.Value,
        value => Avatar.Create(value).Value);
}
