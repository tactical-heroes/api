using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;

public sealed class GetPersonalFileDetailsQueryValidator
    : AbstractValidator<GetPersonalFileDetailsQuery>
{
    public GetPersonalFileDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
