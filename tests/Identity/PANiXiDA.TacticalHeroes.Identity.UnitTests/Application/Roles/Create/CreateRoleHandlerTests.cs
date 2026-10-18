using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Create;

public sealed class CreateRoleHandlerTests
{
    [Theory(
        DisplayName = "Create role handler should delegate creation to the write repository when repository succeeds")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnRoleId_When_RepositorySucceeds(bool isDefault)
    {
        var roleId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IRolesRepository>();
        repository.GetDefaultAsync(Arg.Any<CancellationToken>()).Returns(Result.Success<Role?>(value: null));
        repository.AddAsync(
            Arg.Is<Role>(role => role.Name.Value == "developer"
                && role.IsDefault == isDefault
                && role.Claims.Any(
                    claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            Arg
                .Any<CancellationToken>())
            .Returns(Result.Success(roleId));
        var handler = new CreateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateRoleCommand("Developer", isDefault, claims),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(roleId);
        await repository.Received(1).AddAsync(
            Arg.Is<Role>(role => role.Name.Value == "developer"
                && role.IsDefault == isDefault
                && role.Claims.Any(
                    claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            cancellationToken);
    }

    [Theory(
        DisplayName = "Create role handler should reject invalid values without persistence when command is invalid")]
    [InlineData("", "permission", "heroes.manage")]
    [InlineData("Developer", "", "")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutPersisting_When_CommandIsInvalid(
        string name,
        string claimType,
        string claimValue)
    {
        var repository = Substitute.For<IRolesRepository>();
        repository.GetDefaultAsync(Arg.Any<CancellationToken>()).Returns(Result.Success<Role?>(value: null));
        var handler = new CreateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateRoleCommand(name, false, [new Claim(claimType, claimValue)]),
            cancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        await repository.DidNotReceiveWithAnyArgs().AddAsync(null!, cancellationToken);
    }

    [Fact(DisplayName = "Create role handler should return conflict when another default exists")]
    public async Task HandleAsync_Should_ReturnConflict_When_AnotherDefaultExists()
    {
        var repository = Substitute.For<IRolesRepository>();
        var current = Role.Create(
            RoleId.New(), RoleName.Create("Player").Value, RoleDefaultFlag.Create(true).Value, []);
        repository.GetDefaultAsync(Arg.Any<CancellationToken>()).Returns(Result.Success<Role?>(current));
        var handler = new CreateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new CreateRoleCommand("CustomDefault", true, []), cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        await repository.DidNotReceiveWithAnyArgs().AddAsync(null!, cancellationToken);
    }

    [Fact(DisplayName = "Create role handler should propagate errors when default lookup fails")]
    public async Task HandleAsync_Should_PropagateErrors_When_DefaultLookupFails()
    {
        var repository = Substitute.For<IRolesRepository>();
        var error = Error.Validation(message: "Invalid default role.");
        repository.GetDefaultAsync(Arg.Any<CancellationToken>()).Returns(Result.Failure<Role?>(error));
        var handler = new CreateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new CreateRoleCommand("CustomDefault", true, []), cancellationToken);

        result.Errors.ShouldBe([error]);
        await repository.DidNotReceiveWithAnyArgs().AddAsync(null!, cancellationToken);
    }
}
