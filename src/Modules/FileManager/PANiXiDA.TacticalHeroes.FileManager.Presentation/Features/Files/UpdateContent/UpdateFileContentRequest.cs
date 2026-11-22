using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.UpdateContent;

public sealed record UpdateFileContentRequest([FromForm] IFormFile File);
