namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Matchmaking Integration";
}
