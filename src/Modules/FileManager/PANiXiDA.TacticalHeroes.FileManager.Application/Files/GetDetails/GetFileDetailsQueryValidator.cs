using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;

public sealed class GetFileDetailsQueryValidator
    : AbstractValidator<GetFileDetailsQuery>
{
    public GetFileDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
