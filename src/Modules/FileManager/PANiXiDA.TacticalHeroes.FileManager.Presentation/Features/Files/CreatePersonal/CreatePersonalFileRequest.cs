using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.CreatePersonal;

public sealed record CreatePersonalFileRequest(
    [FromForm] IFormFile File,
    [FromForm] Guid? FolderId = null);
