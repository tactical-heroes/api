using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Models;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonalUploadUrl;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonalUploadUrl;

[Mapper]
internal static partial class CreatePersonalFileUploadUrlMapper
{
    internal static partial CreatePersonalFileUploadUrlCommand ToCommand(CreatePersonalFileUploadUrlRequest request);

    [MapProperty(nameof(@FileUploadUrl.Upload.Url), nameof(CreatePersonalFileUploadUrlResponse.Url))]
    [MapProperty(nameof(@FileUploadUrl.Upload.ExpiresAt), nameof(CreatePersonalFileUploadUrlResponse.ExpiresAt))]
    [MapProperty(
        nameof(@FileUploadUrl.Upload.RequiredHeaders),
        nameof(CreatePersonalFileUploadUrlResponse.RequiredHeaders))]
    internal static partial CreatePersonalFileUploadUrlResponse ToResponse(FileUploadUrl upload);
}
