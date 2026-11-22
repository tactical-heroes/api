namespace PANiXiDA.TacticalHeroes.Battle.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollectionDefinition
    : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Battle Functional";
}
