using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetAccountsMapper
{
    internal static partial PaginationResult<AccountListItemResponse> ToResponse(
        PaginationResult<AccountListItemReadModel> page);
}
