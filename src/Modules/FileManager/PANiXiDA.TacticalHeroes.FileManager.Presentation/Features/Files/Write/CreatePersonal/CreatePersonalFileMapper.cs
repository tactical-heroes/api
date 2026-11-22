using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.CreatePersonal;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.CreatePersonal;

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
