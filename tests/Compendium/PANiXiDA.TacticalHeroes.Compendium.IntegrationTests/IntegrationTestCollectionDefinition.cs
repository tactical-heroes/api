namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Compendium Integration";
}
