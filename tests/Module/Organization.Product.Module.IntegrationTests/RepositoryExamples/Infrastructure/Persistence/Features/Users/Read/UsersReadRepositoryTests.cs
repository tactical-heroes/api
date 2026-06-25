using Microsoft.Extensions.DependencyInjection;

using Organization.Product.Module.Application.Users;
using Organization.Product.Module.Application.Users.Abstractions;
using Organization.Product.Module.Domain.Users;
using Organization.Product.Module.Domain.Users.Abstractions;

namespace Organization.Product.Module.IntegrationTests.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "GetByIdAsync should return user details when user exists")]
    public async Task GetByIdAsync_Should_Return_User_Details_When_User_Exists()
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
        await AddUsersAsync(cancellationToken, user);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();

        var readModel = await repository.GetByIdAsync(user.Id.Value, cancellationToken);

        readModel.ShouldNotBeNull();
        readModel.Id.ShouldBe(user.Id.Value);
        readModel.Name.ShouldBe("John Doe");
        readModel.Email.ShouldBe("john@example.com");
        readModel.Phone.ShouldBe("+12345678901");
        readModel.BirthDate.ShouldBe(birthDate);
        readModel.Avatar.ShouldBe("https://example.com/avatar.png");
    }

    [Fact(DisplayName = "GetPagedListAsync should filter and sort users when role filter is provided")]
    public async Task GetPagedListAsync_Should_Filter_And_Sort_Users_When_Role_Filter_Is_Provided()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstAdmin = User.Create(
            role: "Admin",
            name: "Charlie Admin",
            email: "charlie@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var secondAdmin = User.Create(
            role: "Admin",
            name: "Alice Admin",
            email: "alice@example.com",
            phone: "+12345678902",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var regularUser = User.Create(
            role: "User",
            name: "Bob User",
            email: "bob@example.com",
            phone: "+12345678903",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        await AddUsersAsync(
            cancellationToken,
            firstAdmin,
            secondAdmin,
            regularUser);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
        var filterParameters = new UsersFilterParameters(Role: "admin");
        var paginationParameters = new PaginationParameters(PageNumber: 1, PageSize: 10);
        var sortParameters = new SortParameters(Field: "Name", Order: SortOrder.Ascending);

        var result = await repository.GetPagedListAsync(
            filterParameters,
            paginationParameters,
            sortParameters,
            cancellationToken);

        result.TotalCount.ShouldBe(2);
        result.Items.Select(item => item.Name).ShouldBe(["Alice Admin", "Charlie Admin"]);
        result.Items.ShouldAllBe(item => item.Email.EndsWith("@example.com"));
    }

    [Fact(DisplayName = "GetPagedListAsync should return requested page when users exist")]
    public async Task GetPagedListAsync_Should_Return_Requested_Page_When_Users_Exist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstUser = User.Create(
            role: "User",
            name: "Alice User",
            email: "alice@example.com",
            phone: "+12345678901",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var secondUser = User.Create(
            role: "User",
            name: "Bob User",
            email: "bob@example.com",
            phone: "+12345678902",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        var thirdUser = User.Create(
            role: "User",
            name: "Charlie User",
            email: "charlie@example.com",
            phone: "+12345678903",
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            avatar: "https://example.com/avatar.png").Value;
        await AddUsersAsync(
            cancellationToken,
            firstUser,
            secondUser,
            thirdUser);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersReadRepository>();
        var filterParameters = new UsersFilterParameters(Role: null);
        var paginationParameters = new PaginationParameters(PageNumber: 2, PageSize: 1);
        var sortParameters = new SortParameters(Field: "Name", Order: SortOrder.Ascending);

        var result = await repository.GetPagedListAsync(
            filterParameters,
            paginationParameters,
            sortParameters,
            cancellationToken);

        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(1);
        result.TotalCount.ShouldBe(3);
        result.TotalPages.ShouldBe(3);
        result.Items.ShouldHaveSingleItem().Name.ShouldBe("Bob User");
    }

    private async Task AddUsersAsync(
        CancellationToken cancellationToken,
        params User[] users)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                foreach (var user in users)
                {
                    await repository.AddAsync(user, ct);
                }
            },
            cancellationToken);
    }
}
