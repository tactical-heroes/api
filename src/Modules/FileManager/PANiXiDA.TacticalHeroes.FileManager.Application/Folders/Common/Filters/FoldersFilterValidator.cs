using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Common.Filters;

public sealed class FoldersFilterValidator : AbstractValidator<FoldersFilter>
{
    public FoldersFilterValidator()
    {
        RuleFor(filter => filter.AllowedFileType)
            .MustBeValidDomainValue(FileType.CreateOptional);

        RuleFor(filter => filter.UserId)
            .MustBeValidDomainValue(value => UserId.Create(value.GetValueOrDefault()))
            .When(filter => filter.UserId.HasValue);

        RuleFor(filter => filter.Search == null ? null : filter.Search.Trim())
            .Length(3, FolderName.MaxLength)
            .OverridePropertyName(nameof(FoldersFilter.Search));
    }
}
