using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Write.Mappers;

public sealed class ApplicationRoleMapperTests
{
    [Fact(DisplayName = "Role mapper should collect invalid identity fields when identity is invalid")]
    public void ToDomain_Should_ReturnValidationFailures_When_IdentityIsInvalid()
    {
        var result = ApplicationRoleMapper.ToDomain(CreateDbModel(Guid.Empty, string.Empty, []));

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "Role mapper should reject invalid claims when claim is invalid")]
    public void ToDomain_Should_ReturnValidationFailures_When_ClaimIsInvalid()
    {
        var result = ApplicationRoleMapper.ToDomain(CreateDbModel(Guid.CreateVersion7(), "Developer", [("", "")]));

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Theory(DisplayName = "Role mapper should normalize and deduplicate claims when values are valid")]
    [InlineData(false)]
    [InlineData(true)]
    public void ToDomain_Should_NormalizeAndDeduplicateClaims_When_ValuesAreValid(bool isDefault)
    {
        var dbModel = CreateDbModel(
            Guid.CreateVersion7(),
            " Developer ",
            [(" permission ", " heroes.manage "), ("permission", "heroes.manage")]);
        dbModel.IsDefault = isDefault;

        var result = ApplicationRoleMapper.ToDomain(dbModel);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.Value.ShouldBe("Developer");
        result.Value.IsDefault.ShouldBe(isDefault);
        ApplicationRoleMapper.ToDbModel(result.Value, DateTime.UtcNow, DateTime.UtcNow)
            .IsDefault.ShouldBe(isDefault);
        var claim = result.Value.Claims.ShouldHaveSingleItem();
        claim.Type.Value.ShouldBe("permission");
        claim.Value.Value.ShouldBe("heroes.manage");
    }

    private static ApplicationRole CreateDbModel(
        Guid id,
        string name,
        IEnumerable<(string Type, string Value)> claims)
    {
        return new ApplicationRole
        {
            Id = id,
            Name = name,
            Claims = [.. claims.Select(claim => new ApplicationRoleClaim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            })]
        };
    }
}
