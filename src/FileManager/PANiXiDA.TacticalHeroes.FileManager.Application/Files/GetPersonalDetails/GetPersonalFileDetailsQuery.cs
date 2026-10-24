namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;

public sealed record GetPersonalFileDetailsQuery(Guid Id)
    : IQuery<Result<PersonalFileDetailsReadModel>>;
