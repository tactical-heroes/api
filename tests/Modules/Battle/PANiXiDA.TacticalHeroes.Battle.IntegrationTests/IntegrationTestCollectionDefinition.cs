namespace PANiXiDA.TacticalHeroes.Modules.Battle.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Battle Integration";
}
