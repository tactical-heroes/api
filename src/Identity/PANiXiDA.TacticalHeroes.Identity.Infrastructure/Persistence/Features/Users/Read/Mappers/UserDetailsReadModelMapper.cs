using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class UserDetailsReadModelMapper
    : IReadModelMapper<Guid, UserReadDbModel, UserDetailsReadModel>
{
    [MapProperty(
        nameof(UserReadDbModel.Email),
        nameof(UserDetailsReadModel.Email),
        SuppressNullMismatchDiagnostic = true)]
    [MapProperty(
        nameof(UserReadDbModel.UserName),
        nameof(UserDetailsReadModel.UserName),
        SuppressNullMismatchDiagnostic = true)]
    [MapProperty(
        nameof(UserReadDbModel.EmailConfirmed),
        nameof(UserDetailsReadModel.IsConfirmed))]
    [MapProperty(
        nameof(UserReadDbModel.Status),
        nameof(UserDetailsReadModel.StatusDisplayName),
        Use = nameof(ToStatusDisplayName))]
    [MapperIgnoreTarget(nameof(UserDetailsReadModel.IsBlocked))]
    private static partial UserDetailsReadModel ToReadModel(UserReadDbModel user);

    public static partial IQueryable<UserDetailsReadModel> ProjectTo(
        IQueryable<UserReadDbModel> query);

    [MapProperty(
        nameof(UserClaimReadDbModel.ClaimType),
        "type",
        SuppressNullMismatchDiagnostic = true)]
    [MapProperty(
        nameof(UserClaimReadDbModel.ClaimValue),
        "value",
        SuppressNullMismatchDiagnostic = true)]
    [MapperIgnoreTarget(nameof(Claim.Issuer))]
    [MapperIgnoreTarget(nameof(Claim.OriginalIssuer))]
    [MapperIgnoreTarget(nameof(Claim.Properties))]
    [MapperIgnoreTarget(nameof(Claim.Subject))]
    [MapperIgnoreTarget(nameof(Claim.ValueType))]
    private static partial Claim ToClaim(UserClaimReadDbModel claim);

    [UserMapping(Default = false)]
    private static string ToStatusDisplayName(string status) =>
        UserStatus.FromName(name: status).DisplayName;
}
