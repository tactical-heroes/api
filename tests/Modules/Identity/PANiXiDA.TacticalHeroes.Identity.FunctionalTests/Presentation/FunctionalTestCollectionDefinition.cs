namespace PANiXiDA.TacticalHeroes.Modules.Identity.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class FunctionalTestCollectionDefinition : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Functional";
}
