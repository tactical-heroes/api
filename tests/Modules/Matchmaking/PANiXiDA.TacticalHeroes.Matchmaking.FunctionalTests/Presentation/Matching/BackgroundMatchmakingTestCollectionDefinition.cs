namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

[CollectionDefinition(Name)]
public sealed class BackgroundMatchmakingTestCollectionDefinition
    : ICollectionFixture<BackgroundMatchmakingTestFixture>
{
    public const string Name = "Scheduled Matchmaking Functional";
}
