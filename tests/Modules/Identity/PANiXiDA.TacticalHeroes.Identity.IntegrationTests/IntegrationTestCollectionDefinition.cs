namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Integration";
}
