namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UserReadDbModel : AuditableReadDbModel<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool ConfirmationStatus { get; set; }

    public ICollection<UserRoleReadDbModel> Roles { get; set; } = [];
    public ICollection<UserClaimReadDbModel> Claims { get; set; } = [];
    public UserConfirmationTokenReadDbModel? ConfirmationToken { get; set; }
    public UserPasswordResetTokenReadDbModel? PasswordResetToken { get; set; }
}
