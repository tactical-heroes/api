using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDownloadUrl;

public sealed class GetFileDownloadUrlQueryValidator
    : AbstractValidator<GetFileDownloadUrlQuery>
{
    public GetFileDownloadUrlQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
