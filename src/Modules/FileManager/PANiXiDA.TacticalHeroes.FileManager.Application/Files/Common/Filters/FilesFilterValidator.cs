using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Common.Filters;

public sealed class FilesFilterValidator : AbstractValidator<FilesFilter>
{
    public FilesFilterValidator()
    {
        RuleFor(filter => filter.Type)
            .MustBeValidDomainValue(FileType.CreateOptional);

        RuleFor(filter => filter.UserId)
            .MustBeValidDomainValue(value => UserId.Create(value.GetValueOrDefault()))
            .When(filter => filter.UserId.HasValue);

        RuleFor(filter => filter.Search == null ? null : filter.Search.Trim())
            .Length(3, FileName.MaxLength)
            .OverridePropertyName(nameof(FilesFilter.Search));
    }
}
