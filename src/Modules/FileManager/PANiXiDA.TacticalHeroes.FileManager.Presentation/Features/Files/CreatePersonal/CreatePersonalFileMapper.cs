using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.CreatePersonal;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.CreatePersonal;

[Mapper]
internal static partial class CreatePersonalFileMapper
{
    [MapProperty(nameof(@CreatePersonalFileRequest.File.FileName), nameof(CreatePersonalFileCommand.Name))]
    [MapProperty(
        nameof(@CreatePersonalFileRequest.File.ContentType),
        nameof(CreatePersonalFileCommand.ContentType))]
    [MapProperty(nameof(@CreatePersonalFileRequest.File.Length), nameof(CreatePersonalFileCommand.Size))]
    internal static partial CreatePersonalFileCommand ToCommand(
        CreatePersonalFileRequest request,
        Stream content);

    internal static partial CreatePersonalFileResponse ToResponse(Guid id);
}
