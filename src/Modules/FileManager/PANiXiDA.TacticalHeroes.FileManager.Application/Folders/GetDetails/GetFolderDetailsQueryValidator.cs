using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetDetails;

public sealed class GetFolderDetailsQueryValidator
    : AbstractValidator<GetFolderDetailsQuery>
{
    public GetFolderDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FolderId.Create);
    }
}
