using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonal;

public sealed record CreatePersonalFileRequest(
    [FromForm] IFormFile File,
    [FromForm] Guid? FolderId = null);
