using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

public sealed class GetPersonalFileDownloadUrlQueryValidator
    : AbstractValidator<GetPersonalFileDownloadUrlQuery>
{
    public GetPersonalFileDownloadUrlQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
