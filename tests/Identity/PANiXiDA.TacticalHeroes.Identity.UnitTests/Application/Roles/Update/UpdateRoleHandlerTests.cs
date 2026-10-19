using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Update;

public sealed class UpdateRoleHandlerTests
{
    [Theory(
        DisplayName = "Update role handler should delegate update to the write repository when repository succeeds")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnSuccess_When_RepositorySucceeds(bool isDefault)
    {
        var roleId = Guid.CreateVersion7();
        IReadOnlyCollection<Claim> claims = [new Claim("permission", "heroes.manage")];
        var repository = Substitute.For<IRolesRepository>();
        var readRepository = Substitute.For<IRolesReadRepository>();
        readRepository.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        readRepository.GetDefaultIdAsync(Arg.Any<CancellationToken>()).Returns((Guid?)null);
        repository.UpdateAsync(
            Arg.Is<Role>(
                role => role.Id.Value == roleId
                    && role.Name.Value == "developer"
                    && role.IsDefault == isDefault
                    && role.Claims.Any(
                        claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new UpdateRoleHandler(repository, readRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateRoleCommand(roleId, "Developer", isDefault, claims),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await readRepository.Received(isDefault ? 1 : 0).GetDefaultIdAsync(cancellationToken);
        await repository.Received(1).UpdateAsync(
            Arg.Is<Role>(
                role => role.Id.Value == roleId
                    && role.Name.Value == "developer"
                    && role.IsDefault == isDefault
                    && role.Claims.Any(
                        claim => claim.Type.Value == "permission" && claim.Value.Value == "heroes.manage")),
            cancellationToken);
    }

    [Theory(
        DisplayName = "Update role handler should reject invalid values without persistence when command is invalid")]
    [InlineData("", "permission", "heroes.manage")]
    [InlineData("Developer", "", "")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutPersisting_When_CommandIsInvalid(
        string name,
        string claimType,
        string claimValue)
    {
        var repository = Substitute.For<IRolesRepository>();
        var readRepository = Substitute.For<IRolesReadRepository>();
        readRepository.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        readRepository.GetDefaultIdAsync(Arg.Any<CancellationToken>()).Returns((Guid?)null);
        var handler = new UpdateRoleHandler(repository, readRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateRoleCommand(Guid.CreateVersion7(), name, false, [new Claim(claimType, claimValue)]),
            cancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        await repository.DidNotReceiveWithAnyArgs().UpdateAsync(null!, cancellationToken);
    }

    [Fact(DisplayName = "Update role handler should return conflict when another default exists")]
    public async Task HandleAsync_Should_ReturnConflict_When_AnotherDefaultExists()
    {
        var repository = Substitute.For<IRolesRepository>();
        var readRepository = Substitute.For<IRolesReadRepository>();
        readRepository.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var currentId = Guid.CreateVersion7();
        readRepository.GetDefaultIdAsync(Arg.Any<CancellationToken>()).Returns((Guid?)currentId);
        var handler = new UpdateRoleHandler(repository, readRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateRoleCommand(Guid.CreateVersion7(), "CustomDefault", true, []), cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        await repository.DidNotReceiveWithAnyArgs().UpdateAsync(null!, cancellationToken);
    }

    [Fact(DisplayName = "Update role handler should preserve the default when current default is edited")]
    public async Task HandleAsync_Should_PreserveDefault_When_CurrentDefaultIsEdited()
    {
        var repository = Substitute.For<IRolesRepository>();
        var readRepository = Substitute.For<IRolesReadRepository>();
        readRepository.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var currentId = Guid.CreateVersion7();
        readRepository.GetDefaultIdAsync(Arg.Any<CancellationToken>()).Returns((Guid?)currentId);
        repository.UpdateAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        var handler = new UpdateRoleHandler(repository, readRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateRoleCommand(currentId, "RenamedDefault", true, []), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(
            Arg.Is<Role>(role => role.Id.Value == currentId && role.IsDefault), cancellationToken);
    }

    [Theory(DisplayName = "Update role handler should return not found when target role is missing")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnNotFound_When_TargetRoleIsMissing(bool defaultExists)
    {
        var repository = Substitute.For<IRolesRepository>();
        var readRepository = Substitute.For<IRolesReadRepository>();
        readRepository.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        readRepository.GetDefaultIdAsync(Arg.Any<CancellationToken>())
            .Returns(defaultExists ? Guid.CreateVersion7() : (Guid?)null);
        var handler = new UpdateRoleHandler(repository, readRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateRoleCommand(Guid.CreateVersion7(), "MissingRole", true, []), cancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
        await readRepository.DidNotReceiveWithAnyArgs().GetDefaultIdAsync(cancellationToken);
        await repository.DidNotReceiveWithAnyArgs().UpdateAsync(null!, cancellationToken);
    }
}
