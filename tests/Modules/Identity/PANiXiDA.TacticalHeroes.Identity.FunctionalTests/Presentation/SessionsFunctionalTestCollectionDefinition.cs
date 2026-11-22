namespace PANiXiDA.TacticalHeroes.Modules.Identity.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class SessionsFunctionalTestCollectionDefinition : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Functional sessions";
}
