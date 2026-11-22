namespace PANiXiDA.TacticalHeroes.Modules.Compendium.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollectionDefinition
    : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Compendium Functional";
}
