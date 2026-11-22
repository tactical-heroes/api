using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildHeroes;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildUnits;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Builds.Domain.Players;
using PANiXiDA.TacticalHeroes.Builds.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Domain.Builds;

public sealed class BuildTests
{
    [Fact(DisplayName = "Build should start with empty selections when created")]
    public void Create_Should_SetOwnerAndEmptySelections_When_Created()
    {
        var playerId = PlayerId.New();

        var build = Build.Create(playerId, BuildName.Create("  Northern army  ").Value);

        build.Id.Value.Version.ShouldBe(7);
        build.PlayerId.ShouldBe(playerId);
        build.Name.Value.ShouldBe("Northern army");
        build.Heroes.ShouldBeEmpty();
        build.Units.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Build should preserve ownership and selections when renamed")]
    public void Rename_Should_PreserveOwnerAndSelections_When_Renamed()
    {
        var build = CreateBuild();
        var playerId = build.PlayerId;
        var heroId = HeroId.New();
        var unitId = UnitId.New();
        build.AddHero(heroId);
        build.AddUnit(unitId);

        build.Rename(BuildName.Create("  Updated army  ").Value);

        build.Name.Value.ShouldBe("Updated army");
        build.PlayerId.ShouldBe(playerId);
        build.Heroes.ShouldHaveSingleItem().HeroId.ShouldBe(heroId);
        build.Units.ShouldHaveSingleItem().UnitId.ShouldBe(unitId);
    }

    [Fact(DisplayName = "Build should preserve selected references when a hero is added")]
    public void AddHero_Should_PreserveReferences_When_AHeroIsAdded()
    {
        var build = CreateBuild();
        var heroId = HeroId.New();

        var result = build.AddHero(heroId);

        result.IsSuccess.ShouldBeTrue();
        var entry = build.Heroes.ShouldHaveSingleItem();
        entry.HeroId.ShouldBe(heroId);
        entry.BuildId.ShouldBe(build.Id);
        entry.Id.Value.Version.ShouldBe(7);
    }

    [Fact(DisplayName = "Build should preserve its hero selection when the same hero is added again")]
    public void AddHero_Should_RejectDuplicate_When_TheSameHeroIsAddedAgain()
    {
        var build = CreateBuild();
        var heroId = HeroId.New();
        build.AddHero(heroId);
        var entryId = build.Heroes.Single().Id;

        var result = build.AddHero(heroId);

        result.ShouldHaveSingleError(ErrorType.Conflict, "Hero is already selected in this build.");
        build.Heroes.ShouldHaveSingleItem().Id.ShouldBe(entryId);
    }

    [Fact(DisplayName = "Build should preserve other selections when hero removal is repeated")]
    public void RemoveHero_Should_PreserveOtherSelections_When_HeroRemovalIsRepeated()
    {
        var build = CreateBuild();
        var removed = HeroId.New();
        var remaining = HeroId.New();
        var unitId = UnitId.New();
        build.AddHero(removed);
        build.AddHero(remaining);
        build.AddUnit(unitId);

        build.RemoveHero(removed);
        build.RemoveHero(removed);
        build.RemoveHero(HeroId.New());

        build.Heroes.ShouldHaveSingleItem().HeroId.ShouldBe(remaining);
        build.Units.ShouldHaveSingleItem().UnitId.ShouldBe(unitId);
    }

    [Fact(DisplayName = "Build should preserve selected references when a unit is added")]
    public void AddUnit_Should_PreserveReferences_When_AUnitIsAdded()
    {
        var build = CreateBuild();
        var unitId = UnitId.New();

        var result = build.AddUnit(unitId);

        result.IsSuccess.ShouldBeTrue();
        var entry = build.Units.ShouldHaveSingleItem();
        entry.UnitId.ShouldBe(unitId);
        entry.BuildId.ShouldBe(build.Id);
        entry.Id.Value.Version.ShouldBe(7);
    }

    [Fact(DisplayName = "Build should preserve its unit selection when the same unit is added again")]
    public void AddUnit_Should_RejectDuplicate_When_TheSameUnitIsAddedAgain()
    {
        var build = CreateBuild();
        var unitId = UnitId.New();
        build.AddUnit(unitId);
        var entryId = build.Units.Single().Id;

        var result = build.AddUnit(unitId);

        result.ShouldHaveSingleError(ErrorType.Conflict, "Unit is already selected in this build.");
        build.Units.ShouldHaveSingleItem().Id.ShouldBe(entryId);
    }

    [Fact(DisplayName = "Build should preserve other selections when unit removal is repeated")]
    public void RemoveUnit_Should_PreserveOtherSelections_When_UnitRemovalIsRepeated()
    {
        var build = CreateBuild();
        var removed = UnitId.New();
        var remaining = UnitId.New();
        var heroId = HeroId.New();
        build.AddUnit(removed);
        build.AddUnit(remaining);
        build.AddHero(heroId);

        build.RemoveUnit(removed);
        build.RemoveUnit(removed);
        build.RemoveUnit(UnitId.New());

        build.Units.ShouldHaveSingleItem().UnitId.ShouldBe(remaining);
        build.Heroes.ShouldHaveSingleItem().HeroId.ShouldBe(heroId);
    }

    [Fact(DisplayName = "Build should protect its selections when collections are accessed")]
    public void Create_Should_ProtectSelections_When_CollectionsAreAccessed()
    {
        var build = CreateBuild();
        build.AddHero(HeroId.New());
        build.AddUnit(UnitId.New());
        Action removeHeroes = () => ((ICollection<BuildHero>)build.Heroes).Clear();
        Action removeUnits = () => ((ICollection<BuildUnit>)build.Units).Clear();

        removeHeroes.ShouldThrow<NotSupportedException>();
        removeUnits.ShouldThrow<NotSupportedException>();

        build.Heroes.Count.ShouldBe(1);
        build.Units.Count.ShouldBe(1);
    }

    private static Build CreateBuild()
    {
        return Build.Create(PlayerId.New(), BuildName.Create("Northern army").Value);
    }
}
