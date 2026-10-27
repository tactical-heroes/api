namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

[CollectionDefinition(Name)]
public sealed class UserChangesFunctionalTestCollectionDefinition : ICollectionFixture<FunctionalTestFixture>
{
    public const string Name = "Functional user changes";
}
