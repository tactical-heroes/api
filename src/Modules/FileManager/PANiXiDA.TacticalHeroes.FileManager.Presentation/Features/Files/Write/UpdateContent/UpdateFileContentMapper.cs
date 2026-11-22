using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.UpdateContent;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.UpdateContent;

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
