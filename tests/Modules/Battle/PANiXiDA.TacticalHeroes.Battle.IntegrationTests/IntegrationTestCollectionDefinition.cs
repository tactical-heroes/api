namespace PANiXiDA.TacticalHeroes.Battle.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Battle Integration";
}
