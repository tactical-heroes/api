using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class RoleDetailsReadModelMapper
    : IReadModelMapper<Guid, RoleReadDbModel, RoleDetailsReadModel>
{
    [MapProperty(
        nameof(RoleReadDbModel.Name),
        nameof(RoleDetailsReadModel.Name),
        SuppressNullMismatchDiagnostic = true)]
    private static partial RoleDetailsReadModel ToReadModel(RoleReadDbModel role);

    public static partial IQueryable<RoleDetailsReadModel> ProjectTo(
        IQueryable<RoleReadDbModel> query);

    [MapProperty(
        nameof(RoleClaimReadDbModel.ClaimType),
        "type",
        SuppressNullMismatchDiagnostic = true)]
    [MapProperty(
        nameof(RoleClaimReadDbModel.ClaimValue),
        "value",
        SuppressNullMismatchDiagnostic = true)]
    [MapperIgnoreTarget(nameof(Claim.Issuer))]
    [MapperIgnoreTarget(nameof(Claim.OriginalIssuer))]
    [MapperIgnoreTarget(nameof(Claim.Properties))]
    [MapperIgnoreTarget(nameof(Claim.Subject))]
    [MapperIgnoreTarget(nameof(Claim.ValueType))]
    private static partial Claim ToClaim(RoleClaimReadDbModel claim);
}
