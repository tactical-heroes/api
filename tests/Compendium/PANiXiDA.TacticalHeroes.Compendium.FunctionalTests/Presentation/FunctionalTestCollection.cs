namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollection
    : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Compendium Functional";
}
