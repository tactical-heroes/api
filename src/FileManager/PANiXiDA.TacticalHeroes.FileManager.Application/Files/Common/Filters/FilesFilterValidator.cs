using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;

public sealed class FilesFilterValidator : AbstractValidator<FilesFilter>
{
    public FilesFilterValidator()
    {
        RuleFor(filter => filter.Type)
            .Custom((value, context) =>
            {
                if (value is null)
                {
                    return;
                }

                var type = FileType.Create(value);
                foreach (var error in type.Errors)
                {
                    context.AddFailure(error.Message);
                }
            });

        RuleFor(filter => filter.Search == null ? null : filter.Search.Trim())
            .Length(3, FileName.MaxLength)
            .OverridePropertyName(nameof(FilesFilter.Search));
    }
}
