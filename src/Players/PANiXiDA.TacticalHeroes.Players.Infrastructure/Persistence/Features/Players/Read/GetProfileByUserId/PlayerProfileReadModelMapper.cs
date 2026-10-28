using PANiXiDA.TacticalHeroes.Players.Application.Players.GetProfileByUserId;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.GetProfileByUserId;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class PlayerProfileReadModelMapper
    : IReadModelMapper<Guid, PlayerReadDbModel, PlayerProfileReadModel>
{
    public static partial IQueryable<PlayerProfileReadModel> ProjectTo(IQueryable<PlayerReadDbModel> query);

    private static partial PlayerProfileReadModel ToReadModel(PlayerReadDbModel player);
}
