namespace PANiXiDA.TacticalHeroes.Players.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollectionDefinition
    : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Players Functional";
}
