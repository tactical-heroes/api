using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Models;

public sealed record FileUploadUrl(Guid Id, PresignedUploadUrl Upload);
