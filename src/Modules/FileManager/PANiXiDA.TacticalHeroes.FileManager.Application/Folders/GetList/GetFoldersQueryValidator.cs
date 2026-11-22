using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;

public sealed class GetFoldersQueryValidator : AbstractValidator<GetFoldersQuery>
{
    public GetFoldersQueryValidator()
    {
        RuleFor(query => query.Filter)
            .NotNull()
            .SetValidator(new FoldersFilterValidator());

        RuleFor(query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(query => query.SortingParameters)
            .NotNull()
            .SetValidator(new FolderListItemReadModelSortingValidator());
    }
}
