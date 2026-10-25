namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Matchmaking Integration";
}
