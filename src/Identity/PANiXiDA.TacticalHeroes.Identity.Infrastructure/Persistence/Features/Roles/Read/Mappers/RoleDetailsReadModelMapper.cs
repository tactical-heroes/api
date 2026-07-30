using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Common;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[UseStaticMapper(typeof(ClaimMapper))]
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
}
