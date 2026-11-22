namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.GetDetails;

public sealed record GetFolderDetailsQuery(Guid Id)
    : IQuery<Result<FolderDetailsReadModel>>;
