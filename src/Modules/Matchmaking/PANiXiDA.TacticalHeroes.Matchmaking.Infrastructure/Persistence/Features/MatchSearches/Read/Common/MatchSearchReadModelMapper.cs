using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.Common;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class MatchSearchReadModelMapper
    : IReadModelMapper<Guid, MatchSearchReadDbModel, MatchSearchReadModel>
{
    public static partial IQueryable<MatchSearchReadModel> ProjectTo(IQueryable<MatchSearchReadDbModel> query);

    private static partial MatchSearchReadModel ToReadModel(MatchSearchReadDbModel search);
}
