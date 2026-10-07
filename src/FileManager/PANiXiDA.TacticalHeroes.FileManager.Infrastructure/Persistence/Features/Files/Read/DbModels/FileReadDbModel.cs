using System.ComponentModel.DataAnnotations;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

public sealed class FileReadDbModel : AuditableReadDbModel<Guid>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Purpose { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ContentType { get; set; }

    public long? Size { get; set; }
    public uint Version { get; set; }
}
