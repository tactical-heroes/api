using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetList;
using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Persistence.Features.Roles.Read.GetList;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class RoleListItemReadModelMapper
    : IReadModelMapper<Guid, RoleReadDbModel, RoleListItemReadModel>
{
    public static partial IQueryable<RoleListItemReadModel> ProjectTo(
        IQueryable<RoleReadDbModel> query);

    [MapProperty(
        nameof(RoleReadDbModel.Name),
        nameof(RoleListItemReadModel.Name),
        SuppressNullMismatchDiagnostic = true)]
    private static partial RoleListItemReadModel ToReadModel(RoleReadDbModel role);
}
