using PANiXiDA.TacticalHeroes.Modules.Players.Application.Players.GetInternalPlayer;
using PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Persistence.Features.Players.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Persistence.Features.Players.Read.GetInternalPlayer;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class PlayerReadModelMapper
    : IReadModelMapper<Guid, PlayerReadDbModel, PlayerReadModel>
{
    public static partial IQueryable<PlayerReadModel> ProjectTo(IQueryable<PlayerReadDbModel> query);

    private static partial PlayerReadModel ToReadModel(PlayerReadDbModel player);
}
