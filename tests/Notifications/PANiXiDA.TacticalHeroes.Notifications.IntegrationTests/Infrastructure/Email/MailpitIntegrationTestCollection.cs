namespace PANiXiDA.TacticalHeroes.Notifications.IntegrationTests.Infrastructure.Email;

[CollectionDefinition(Name)]
public sealed class MailpitIntegrationTestCollection
    : ICollectionFixture<MailpitIntegrationTestFixture>
{
    public const string Name = "Mailpit integration";
}
