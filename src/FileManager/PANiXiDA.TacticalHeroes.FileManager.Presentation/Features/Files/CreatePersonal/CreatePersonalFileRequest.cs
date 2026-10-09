using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonal;

public sealed record CreatePersonalFileRequest(
    IFormFile File,
    Guid? FolderId = null);
