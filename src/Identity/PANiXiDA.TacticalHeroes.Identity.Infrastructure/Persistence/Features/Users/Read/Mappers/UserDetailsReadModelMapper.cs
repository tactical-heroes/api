using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Common;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(ClaimMapper))]
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

    [UserMapping(Default = false)]
    private static string ToStatusDisplayName(string status) =>
        UserStatus.FromName(name: status).DisplayName;
}
