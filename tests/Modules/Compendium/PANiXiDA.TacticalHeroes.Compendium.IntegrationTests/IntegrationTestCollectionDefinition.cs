namespace PANiXiDA.TacticalHeroes.Modules.Compendium.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Compendium Integration";
}
