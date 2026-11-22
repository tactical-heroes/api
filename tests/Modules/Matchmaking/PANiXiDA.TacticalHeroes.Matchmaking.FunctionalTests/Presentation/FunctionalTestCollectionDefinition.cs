namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollectionDefinition
    : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Matchmaking Functional";
}
