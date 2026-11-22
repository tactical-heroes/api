namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration;

[CollectionDefinition(Name)]
public sealed class BackgroundExpirationTestCollectionDefinition
    : ICollectionFixture<BackgroundExpirationTestFixture>
{
    public const string Name = "Scheduled Match Search Expiration Functional";
}
