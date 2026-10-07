using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;

public sealed class FoldersFilterValidator : AbstractValidator<FoldersFilter>
{
    public FoldersFilterValidator()
    {
        RuleFor(filter => filter.Search == null ? null : filter.Search.Trim())
            .Length(3, FolderName.MaxLength)
            .OverridePropertyName(nameof(FoldersFilter.Search));
    }
}
