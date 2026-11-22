using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetDetails;

public sealed class GetFileDetailsQueryValidator
    : AbstractValidator<GetFileDetailsQuery>
{
    public GetFileDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
