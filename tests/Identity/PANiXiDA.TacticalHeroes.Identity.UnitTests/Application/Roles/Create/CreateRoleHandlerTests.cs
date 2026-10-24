using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

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
        var readRepository = Substitute.For<IRolesReadRepository>();
        readRepository.GetDefaultIdAsync(Arg.Any<CancellationToken>()).Returns((Guid?)null);
        repository.AddAsync(
            Arg.Is<Role>(role => role.Name.Value == "Developer"
                && role.IsDefault == isDefault
                && role.Claims.Any(
                    claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            Arg
                .Any<CancellationToken>())
            .Returns(Result.Success(roleId));
        var handler = new CreateRoleHandler(repository, readRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateRoleCommand("Developer", isDefault, claims),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await readRepository.Received(isDefault ? 1 : 0).GetDefaultIdAsync(cancellationToken);
        result.Value.ShouldBe(roleId);
        await repository.Received(1).AddAsync(
            Arg.Is<Role>(role => role.Name.Value == "Developer"
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
        var readRepository = Substitute.For<IRolesReadRepository>();
        readRepository.GetDefaultIdAsync(Arg.Any<CancellationToken>()).Returns((Guid?)null);
        var handler = new CreateRoleHandler(repository, readRepository);
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
        var readRepository = Substitute.For<IRolesReadRepository>();
        var currentId = Guid.CreateVersion7();
        readRepository.GetDefaultIdAsync(Arg.Any<CancellationToken>()).Returns((Guid?)currentId);
        var handler = new CreateRoleHandler(repository, readRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new CreateRoleCommand("CustomDefault", true, []), cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        await repository.DidNotReceiveWithAnyArgs().AddAsync(null!, cancellationToken);
    }
}
