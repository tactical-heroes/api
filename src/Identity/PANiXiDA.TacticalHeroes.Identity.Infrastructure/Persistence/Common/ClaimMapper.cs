using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Common;

[Mapper]
internal static partial class ClaimMapper
{
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(
        nameof(RoleClaimReadDbModel.ClaimType),
        "type",
        SuppressNullMismatchDiagnostic = true)]
    [MapProperty(
        nameof(RoleClaimReadDbModel.ClaimValue),
        "value",
        SuppressNullMismatchDiagnostic = true)]
    [MapperIgnoreSource(nameof(RoleClaimReadDbModel.Id))]
    [MapperIgnoreSource(nameof(RoleClaimReadDbModel.Role))]
    [MapperIgnoreSource(nameof(RoleClaimReadDbModel.RoleId))]
    internal static partial Claim ToClaim(RoleClaimReadDbModel claim);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(
        nameof(UserClaimReadDbModel.ClaimType),
        "type",
        SuppressNullMismatchDiagnostic = true)]
    [MapProperty(
        nameof(UserClaimReadDbModel.ClaimValue),
        "value",
        SuppressNullMismatchDiagnostic = true)]
    [MapperIgnoreSource(nameof(UserClaimReadDbModel.Id))]
    [MapperIgnoreSource(nameof(UserClaimReadDbModel.User))]
    [MapperIgnoreSource(nameof(UserClaimReadDbModel.UserId))]
    internal static partial Claim ToClaim(UserClaimReadDbModel claim);
}
