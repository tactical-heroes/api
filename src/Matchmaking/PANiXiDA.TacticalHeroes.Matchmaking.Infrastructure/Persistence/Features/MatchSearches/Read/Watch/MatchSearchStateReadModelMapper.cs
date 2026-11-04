using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.Watch;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class MatchSearchStateReadModelMapper
    : IReadModelMapper<Guid, MatchSearchReadDbModel, MatchSearchStateReadModel>
{
    public static partial IQueryable<MatchSearchStateReadModel> ProjectTo(IQueryable<MatchSearchReadDbModel> query);

    [MapProperty(nameof(MatchSearchReadDbModel.Id), nameof(MatchSearchStateReadModel.MatchSearchId))]
    private static partial MatchSearchStateReadModel ToReadModel(MatchSearchReadDbModel search);
}
