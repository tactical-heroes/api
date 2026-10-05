using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Create;

public sealed class CreateRoleHandlerTests
{
    [Fact(DisplayName = "Create role handler should delegate creation to the write repository when repository succeeds")]
    public async Task HandleAsync_Should_ReturnRoleId_When_RepositorySucceeds()
    {
        var roleId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IRolesRepository>();
        repository.AddAsync(Arg.Is<Role>(role => role.Name.Value == "admin" && role.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")), Arg.Any<CancellationToken>())
            .Returns(Result.Success(roleId));
        var handler = new CreateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateRoleCommand("admin", claims),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(roleId);
        await repository.Received(1).AddAsync(Arg.Is<Role>(role => role.Name.Value == "admin" && role.Claims.Any(claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")), cancellationToken);
    }

    [Theory(DisplayName = "Create role handler should reject invalid values without persistence when command is invalid")]
    [InlineData("", "permission", "heroes.manage")]
    [InlineData("admin", "", "")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutPersisting_When_CommandIsInvalid(
        string name,
        string claimType,
        string claimValue)
    {
        var repository = Substitute.For<IRolesRepository>();
        var handler = new CreateRoleHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateRoleCommand(name, [new Claim(claimType, claimValue)]),
            cancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        await repository.DidNotReceiveWithAnyArgs().AddAsync(null!, cancellationToken);
    }
}
