using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetDownloadUrl;

public sealed class GetFileDownloadUrlQueryValidator
    : AbstractValidator<GetFileDownloadUrlQuery>
{
    public GetFileDownloadUrlQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
