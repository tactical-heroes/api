namespace PANiXiDA.TacticalHeroes.Modules.Identity.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Integration";
}
