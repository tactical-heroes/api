using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.Watch;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class MatchSearchReadModelMapper
    : IReadModelMapper<Guid, MatchSearchReadDbModel, MatchSearchReadModel>
{
    public static partial IQueryable<MatchSearchReadModel> ProjectTo(IQueryable<MatchSearchReadDbModel> query);

    private static partial MatchSearchReadModel ToReadModel(MatchSearchReadDbModel search);
}
