using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

public sealed class GetFilesQueryValidator : AbstractValidator<GetFilesQuery>
{
    public GetFilesQueryValidator()
    {
        RuleFor(query => query.Filter)
            .NotNull()
            .SetValidator(new FilesFilterValidator());

        RuleFor(query => query.PaginationParameters)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());

        RuleFor(query => query.SortingParameters)
            .NotNull()
            .SetValidator(new FileListItemReadModelSortingValidator());
    }
}
