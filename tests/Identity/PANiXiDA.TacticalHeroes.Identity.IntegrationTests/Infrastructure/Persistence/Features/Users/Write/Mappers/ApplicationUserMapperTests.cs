using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;
using PANiXiDA.TacticalHeroes.Testing.Assertions;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Users.Write.Mappers;

public sealed class ApplicationUserMapperTests
{
    [Fact(DisplayName = "User mapper should collect invalid identity fields when identity is invalid")]
    public void ToDomain_Should_ReturnValidationFailures_When_IdentityIsInvalid()
    {
        var result = ApplicationUserMapper.ToDomain(CreateDbModel(Guid.Empty, "invalid-email", false, [], []));

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "User mapper should reject an empty role id when role id is empty")]
    public void ToDomain_Should_ReturnValidationFailure_When_RoleIdIsEmpty()
    {
        var result = ApplicationUserMapper.ToDomain(CreateDbModel(Guid.CreateVersion7(), "hero@example.com", false, [Guid.Empty], []));

        result.ShouldHaveSingleError(ErrorType.Validation, "Role id cannot be empty.");
    }

    [Fact(DisplayName = "User mapper should reject invalid claims when claim is invalid")]
    public void ToDomain_Should_ReturnValidationFailures_When_ClaimIsInvalid()
    {
        var result = ApplicationUserMapper.ToDomain(CreateDbModel(Guid.CreateVersion7(), "hero@example.com", false, [], [("", "")]));

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "User mapper should preserve confirmation and deduplicate authorization state when values are valid")]
    public void ToDomain_Should_RestoreAuthorizationState_When_ValuesAreValid()
    {
        var roleId = Guid.CreateVersion7();

        var result = ApplicationUserMapper.ToDomain(CreateDbModel(
            Guid.CreateVersion7(), " HERO@Example.com ", true,
            [roleId, roleId], [(" permission ", " heroes.read "), ("permission", "heroes.read")],
            userName: " restored-hero ", status: " Blocked "));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.Value.ShouldBe("hero@example.com");
        result.Value.UserName.Value.ShouldBe("restored-hero");
        result.Value.Status.IsBlocked.ShouldBeTrue();
        result.Value.ConfirmationStatus.IsConfirmed.ShouldBeTrue();
        result.Value.RoleIds.ShouldHaveSingleItem().Value.ShouldBe(roleId);
        result.Value.Claims.ShouldHaveSingleItem().Value.Value.ShouldBe("heroes.read");
        result.Value.GetDomainEvents().ShouldBeEmpty();
    }

    [Theory(DisplayName = "User mapper should reject invalid profile values when profile is invalid")]
    [InlineData("", "Active")]
    [InlineData("hero", "")]
    [InlineData("hero", "Unknown")]
    public void ToDomain_Should_ReturnValidationFailure_When_ProfileIsInvalid(
        string userName,
        string status)
    {
        var dbModel = CreateDbModel(
            Guid.CreateVersion7(), "hero@example.com", false, [], [], userName, status);

        var result = ApplicationUserMapper.ToDomain(dbModel);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Validation);
    }

    private static ApplicationUser CreateDbModel(
        Guid id,
        string email,
        bool isConfirmed,
        IEnumerable<Guid> roleIds,
        IEnumerable<(string Type, string Value)> claims,
        string userName = "hero",
        string status = "Active")
    {
        return new ApplicationUser
        {
            Id = id,
            Email = email,
            UserName = userName,
            Status = status,
            EmailConfirmed = isConfirmed,
            Roles = roleIds.Select(roleId => new ApplicationUserRole
            {
                UserId = id,
                RoleId = roleId
            }).ToList(),
            Claims = claims.Select(claim => new ApplicationUserClaim
            {
                UserId = id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            }).ToList()
        };
    }
}
