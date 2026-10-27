using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildHeroes;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildUnits;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Builds.Domain.Players;
using PANiXiDA.TacticalHeroes.Builds.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Builds.Domain.Builds;

public sealed class Build : AggregateRoot<BuildId>
{
    private readonly List<BuildHero> _heroes = [];
    private readonly List<BuildUnit> _units = [];

    private Build(BuildId id, PlayerId playerId, BuildName name) : base(id)
    {
        PlayerId = playerId;
        Name = name;
    }

    public PlayerId PlayerId { get; private set; }
    public BuildName Name { get; private set; }

    public IReadOnlyCollection<BuildHero> Heroes => _heroes.AsReadOnly();
    public IReadOnlyCollection<BuildUnit> Units => _units.AsReadOnly();

    public static Build Create(PlayerId playerId, BuildName name)
    {
        return new Build(id: BuildId.New(), playerId: playerId, name: name);
    }

    public void Rename(BuildName name)
    {
        Name = name;
    }

    public Result AddHero(HeroId heroId)
    {
        if (_heroes.Any(hero => hero.HeroId == heroId))
        {
            return Result.Failure(Error.Conflict("Hero is already selected in this build."));
        }

        _heroes.Add(BuildHero.Create(Id, heroId));

        return Result.Success();
    }

    public void RemoveHero(HeroId heroId)
    {
        _heroes.RemoveAll(hero => hero.HeroId == heroId);
    }

    public Result AddUnit(UnitId unitId)
    {
        if (_units.Any(unit => unit.UnitId == unitId))
        {
            return Result.Failure(Error.Conflict("Unit is already selected in this build."));
        }

        _units.Add(BuildUnit.Create(Id, unitId));

        return Result.Success();
    }

    public void RemoveUnit(UnitId unitId)
    {
        _units.RemoveAll(unit => unit.UnitId == unitId);
    }
}
