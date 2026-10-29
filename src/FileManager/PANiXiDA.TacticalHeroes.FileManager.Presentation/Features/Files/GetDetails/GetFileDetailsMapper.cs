using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDetails;

[Mapper]
internal static partial class GetFileDetailsMapper
{
    internal static partial GetFileDetailsQuery ToQuery(Guid id);

    [MapperIgnoreSource(nameof(FileDetailsReadModel.UserId))]
    internal static partial GetFileDetailsResponse ToResponse(FileDetailsReadModel file);
}
