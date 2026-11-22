namespace PANiXiDA.TacticalHeroes.Modules.Builds.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollectionDefinition
    : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Builds Functional";
}
