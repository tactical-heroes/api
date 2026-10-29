using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.Process;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class SearchingMatchSearchReadModelMapper
    : IReadModelMapper<Guid, MatchSearchReadDbModel, SearchingMatchSearchReadModel>
{
    public static partial IQueryable<SearchingMatchSearchReadModel> ProjectTo(IQueryable<MatchSearchReadDbModel> query);

    private static partial SearchingMatchSearchReadModel ToReadModel(MatchSearchReadDbModel search);
}
