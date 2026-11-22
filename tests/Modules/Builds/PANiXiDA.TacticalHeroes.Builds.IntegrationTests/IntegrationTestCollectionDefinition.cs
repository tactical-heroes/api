namespace PANiXiDA.TacticalHeroes.Modules.Builds.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Builds Integration";
}
