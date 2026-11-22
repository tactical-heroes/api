namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Read.GetDetails;

public sealed record GetFolderDetailsQuery(Guid Id)
    : IQuery<Result<FolderDetailsReadModel>>;
