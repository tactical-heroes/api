namespace PANiXiDA.TacticalHeroes.Modules.Players.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Players Integration";
}
