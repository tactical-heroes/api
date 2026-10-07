using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;

public sealed class FilesFilterValidator : AbstractValidator<FilesFilter>
{
    public FilesFilterValidator()
    {
        RuleFor(filter => filter.Search == null ? null : filter.Search.Trim())
            .Length(
                3,
                FileName.MaxLength)
            .OverridePropertyName(nameof(FilesFilter.Search));
    }
}
