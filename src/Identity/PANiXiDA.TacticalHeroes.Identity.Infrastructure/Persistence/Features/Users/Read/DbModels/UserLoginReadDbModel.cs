namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

public sealed class UserLoginReadDbModel
{
    public string LoginProvider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string? ProviderDisplayName { get; set; }
    public Guid UserId { get; set; }

    public UserReadDbModel? User { get; set; }
}
