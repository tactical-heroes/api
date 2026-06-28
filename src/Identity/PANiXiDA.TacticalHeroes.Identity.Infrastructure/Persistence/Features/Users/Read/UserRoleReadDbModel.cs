namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UserRoleReadDbModel
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
