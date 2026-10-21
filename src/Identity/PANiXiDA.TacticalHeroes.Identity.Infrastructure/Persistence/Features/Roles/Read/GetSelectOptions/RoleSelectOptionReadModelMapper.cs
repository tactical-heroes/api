using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetSelectOptions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.GetSelectOptions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class RoleSelectOptionReadModelMapper
    : IReadModelMapper<Guid, RoleReadDbModel, RoleSelectOptionReadModel>
{
    public static partial IQueryable<RoleSelectOptionReadModel> ProjectTo(IQueryable<RoleReadDbModel> query);

    [MapProperty(
        nameof(RoleReadDbModel.Name),
        nameof(RoleSelectOptionReadModel.Name),
        SuppressNullMismatchDiagnostic = true)]
    private static partial RoleSelectOptionReadModel ToReadModel(RoleReadDbModel role);
}
