using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Abstractions;
using Organization.Product.Module.Domain.Users.Enumerations;

namespace Organization.Product.Module.IntegrationTests.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddAsync should persist user when transaction is committed")]
    public async Task AddAsync_Should_Persist_User_When_Transaction_Is_Committed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-35);
        var user = User.Create(
            role: "Admin",
            name: "John Doe",
            email: "john@example.com",
            phone: "+12345678901",
            birthDate: birthDate,
            avatar: "https://example.com/avatar.png").Value;

        await AddUserAsync(user, cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var persistedUser = await repository.GetByIdAsync(user.Id, cancellationToken);

        persistedUser.ShouldNotBeNull();
        persistedUser.Id.ShouldBe(user.Id);
        persistedUser.Role.ShouldBe(UserRole.Admin);
        persistedUser.Name.Value.ShouldBe("John Doe");
        persistedUser.Email.Value.ShouldBe("john@example.com");
        persistedUser.Phone.Value.ShouldBe("+12345678901");
        persistedUser.BirthDate.Value.ShouldBe(birthDate);
        persistedUser.Avatar.ShouldNotBeNull();
        persistedUser.Avatar.Value.ShouldBe("https://example.com/avatar.png");
    }

    [Fact(DisplayName = "UpdateAsync should persist user changes when transaction is committed")]
    public async Task UpdateAsync_Should_Persist_User_Changes_When_Transaction_Is_Committed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = User.Create(
            role: "User",
            name: "John Doe",
            email: "old@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var updatedBirthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-40);
        await AddUserAsync(user, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var persistedUser = await repository.GetByIdAsync(user.Id, cancellationToken);
            persistedUser.ShouldNotBeNull();

            var updateResult = persistedUser.Update(
                role: "Moderator",
                name: "Jane Doe",
                email: "new@example.com",
                phone: "+19876543210",
                birthDate: updatedBirthDate,
                avatar: null);

            updateResult.IsSuccess.ShouldBeTrue();

            await unitOfWork.ExecuteInTransactionAsync(
                ct => repository.UpdateAsync(persistedUser, ct),
                cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationRepository = verificationScope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var updatedUser = await verificationRepository.GetByIdAsync(user.Id, cancellationToken);

        updatedUser.ShouldNotBeNull();
        updatedUser.Role.ShouldBe(UserRole.Moderator);
        updatedUser.Name.Value.ShouldBe("Jane Doe");
        updatedUser.Email.Value.ShouldBe("new@example.com");
        updatedUser.Phone.Value.ShouldBe("+19876543210");
        updatedUser.BirthDate.Value.ShouldBe(updatedBirthDate);
        updatedUser.Avatar.ShouldBeNull();
    }

    [Fact(DisplayName = "DeleteAsync should remove user when transaction is committed")]
    public async Task DeleteAsync_Should_Remove_User_When_Transaction_Is_Committed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = User.Create(
            role: "User",
            name: "John Doe",
            email: "john.doe@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        await AddUserAsync(user, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var persistedUser = await repository.GetByIdAsync(user.Id, cancellationToken);
            persistedUser.ShouldNotBeNull();

            await unitOfWork.ExecuteInTransactionAsync(
                ct => repository.DeleteAsync(persistedUser, ct),
                cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationRepository = verificationScope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var deletedUser = await verificationRepository.GetByIdAsync(user.Id, cancellationToken);

        deletedUser.ShouldBeNull();
    }

    [Fact(DisplayName = "AddAsync should throw DbUpdateException when email is duplicated")]
    public async Task AddAsync_Should_Throw_DbUpdateException_When_Email_Is_Duplicated()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstUser = User.Create(
            role: "User",
            name: "John Doe",
            email: "same@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var secondUser = User.Create(
            role: "User",
            name: "John Doe",
            email: "same@example.com",
            phone: "+12345678902",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        await AddUserAsync(firstUser, cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var exception = await Should.ThrowAsync<DbUpdateException>(
            () => unitOfWork.ExecuteInTransactionAsync(
                ct => repository.AddAsync(secondUser, ct),
                cancellationToken));

        exception.InnerException.ShouldNotBeNull();
    }

    private async Task AddUserAsync(
        User user,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await unitOfWork.ExecuteInTransactionAsync(
            ct => repository.AddAsync(user, ct),
            cancellationToken);
    }
}
