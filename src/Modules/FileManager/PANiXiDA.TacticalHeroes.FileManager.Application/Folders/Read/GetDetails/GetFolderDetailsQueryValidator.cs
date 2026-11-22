using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Read.GetDetails;

public sealed class GetFolderDetailsQueryValidator
    : AbstractValidator<GetFolderDetailsQuery>
{
    public GetFolderDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FolderId.Create);
    }
}
