using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetFactionsMapper
{
    internal static partial GetFactionsQuery ToQuery(
        PaginationParameters pagination);

    internal static partial PaginationResult<FactionListItemResponse> ToResponse(
        PaginationResult<FactionListItemReadModel> page);
}
