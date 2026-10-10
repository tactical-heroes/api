using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonal;

[Mapper]
internal static partial class CreatePersonalFileMapper
{
    [MapProperty(nameof(CreatePersonalFileRequest.File) + ".FileName", nameof(CreatePersonalFileCommand.Name))]
    [MapProperty(nameof(CreatePersonalFileRequest.File) + ".ContentType", nameof(CreatePersonalFileCommand.ContentType))]
    [MapProperty(nameof(CreatePersonalFileRequest.File) + ".Length", nameof(CreatePersonalFileCommand.Size))]
    internal static partial CreatePersonalFileCommand ToCommand(
        CreatePersonalFileRequest request,
        Stream content,
        Guid userId);

    internal static partial CreatePersonalFileResponse ToResponse(Guid id);
}
