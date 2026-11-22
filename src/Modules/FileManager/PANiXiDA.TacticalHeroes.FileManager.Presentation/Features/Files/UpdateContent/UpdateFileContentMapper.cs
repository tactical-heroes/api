using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.UpdateContent;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.UpdateContent;

[Mapper]
internal static partial class UpdateFileContentMapper
{
    [MapProperty(
        nameof(@UpdateFileContentRequest.File.ContentType),
        nameof(UpdateFileContentCommand.ContentType))]
    [MapProperty(nameof(@UpdateFileContentRequest.File.Length), nameof(UpdateFileContentCommand.Size))]
    internal static partial UpdateFileContentCommand ToCommand(
        UpdateFileContentRequest request, Guid id, Stream content);
}
