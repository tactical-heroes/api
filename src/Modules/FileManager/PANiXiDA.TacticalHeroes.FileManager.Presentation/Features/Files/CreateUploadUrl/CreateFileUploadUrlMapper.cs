using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Models;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreateUploadUrl;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreateUploadUrl;

[Mapper]
internal static partial class CreateFileUploadUrlMapper
{
    internal static partial CreateFileUploadUrlCommand ToCommand(CreateFileUploadUrlRequest request);

    [MapProperty(nameof(@FileUploadUrl.Upload.Url), nameof(CreateFileUploadUrlResponse.Url))]
    [MapProperty(nameof(@FileUploadUrl.Upload.ExpiresAt), nameof(CreateFileUploadUrlResponse.ExpiresAt))]
    [MapProperty(
        nameof(@FileUploadUrl.Upload.RequiredHeaders),
        nameof(CreateFileUploadUrlResponse.RequiredHeaders))]
    internal static partial CreateFileUploadUrlResponse ToResponse(FileUploadUrl upload);
}
