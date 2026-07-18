using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class UserListItemReadModelMapper
    : IReadModelMapper<Guid, UserReadDbModel, UserListItemReadModel>
{
    [MapProperty(
        nameof(UserReadDbModel.Email),
        nameof(UserListItemReadModel.Email),
        SuppressNullMismatchDiagnostic = true)]
    [MapProperty(
        nameof(UserReadDbModel.UserName),
        nameof(UserListItemReadModel.UserName),
        SuppressNullMismatchDiagnostic = true)]
    [MapProperty(
        nameof(UserReadDbModel.EmailConfirmed),
        nameof(UserListItemReadModel.IsConfirmed))]
    [MapProperty(
        nameof(UserReadDbModel.Status),
        nameof(UserListItemReadModel.StatusDisplayName),
        Use = nameof(ToStatusDisplayName))]
    private static partial UserListItemReadModel ToReadModel(UserReadDbModel user);

    public static partial IQueryable<UserListItemReadModel> ProjectTo(
        IQueryable<UserReadDbModel> query);

    [UserMapping(Default = false)]
    private static string ToStatusDisplayName(string status) =>
        UserStatus.FromName(name: status).DisplayName;
}
