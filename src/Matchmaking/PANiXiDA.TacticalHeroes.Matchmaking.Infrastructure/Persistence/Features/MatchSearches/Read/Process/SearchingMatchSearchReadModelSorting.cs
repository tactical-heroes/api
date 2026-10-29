using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.Process;

internal sealed partial class SearchingMatchSearchReadModelSorting
    : IReadModelSorting<SearchingMatchSearchReadModel>
{
    public static SortingParameters DefaultSorting { get; } = SortingParameters.Of(
        new SortField(nameof(SearchingMatchSearchReadModel.QueuedAt)),
        new SortField(nameof(SearchingMatchSearchReadModel.Id)));
}
