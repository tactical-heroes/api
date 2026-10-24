using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetPersonalDetails;

[Mapper]
internal static partial class GetPersonalFileDetailsMapper
{
    internal static partial GetPersonalFileDetailsQuery ToQuery(Guid id);

    internal static partial GetPersonalFileDetailsResponse ToResponse(PersonalFileDetailsReadModel file);
}
