namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Compendium Integration";
}
