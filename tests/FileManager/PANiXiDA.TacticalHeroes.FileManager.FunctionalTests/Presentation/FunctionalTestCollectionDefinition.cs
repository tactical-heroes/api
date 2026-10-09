namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollectionDefinition
    : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "FileManager Functional";
}
