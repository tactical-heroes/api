namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "FileManager Integration";
}
