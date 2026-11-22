namespace PANiXiDA.TacticalHeroes.Builds.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollectionDefinition
    : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Builds Functional";
}
