using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Create;

public sealed record CreateFileRequest(
    [FromForm] IFormFile File,
    [FromForm] string Type,
    [FromForm] Guid? UserId = null,
    [FromForm] Guid? FolderId = null);
