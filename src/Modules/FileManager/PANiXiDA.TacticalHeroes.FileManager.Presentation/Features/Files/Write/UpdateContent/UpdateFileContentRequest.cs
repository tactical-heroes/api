using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.UpdateContent;

public sealed record UpdateFileContentRequest([FromForm] IFormFile File);
