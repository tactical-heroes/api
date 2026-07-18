using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class RoleListItemReadModelMapper
    : IReadModelMapper<Guid, RoleReadDbModel, RoleListItemReadModel>
{
    [MapProperty(
        nameof(RoleReadDbModel.Name),
        nameof(RoleListItemReadModel.Name),
        SuppressNullMismatchDiagnostic = true)]
    private static partial RoleListItemReadModel ToReadModel(RoleReadDbModel role);

    public static partial IQueryable<RoleListItemReadModel> ProjectTo(
        IQueryable<RoleReadDbModel> query);
}
