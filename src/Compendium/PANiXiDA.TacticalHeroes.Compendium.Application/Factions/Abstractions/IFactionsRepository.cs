using PANiXiDA.Core.Domain.Abstractions;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

public interface IFactionsRepository : IRepository<FactionId, Faction>;
