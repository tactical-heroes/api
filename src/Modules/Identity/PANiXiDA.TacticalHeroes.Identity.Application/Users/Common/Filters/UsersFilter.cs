namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Common.Filters;

public sealed record UsersFilter(string? Email = null) : IFilter;
