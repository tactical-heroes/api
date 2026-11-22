using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Create;

[Mapper]
internal static partial class CreateFileMapper
{
    [MapProperty(nameof(@CreateFileRequest.File.FileName), nameof(CreateFileCommand.Name))]
    [MapProperty(nameof(@CreateFileRequest.File.ContentType), nameof(CreateFileCommand.ContentType))]
    [MapProperty(nameof(@CreateFileRequest.File.Length), nameof(CreateFileCommand.Size))]
    internal static partial CreateFileCommand ToCommand(CreateFileRequest request, Stream content);

    internal static partial CreateFileResponse ToResponse(Guid id);
}
