namespace PANiXiDA.TacticalHeroes.Modules.Notifications.IntegrationTests.Infrastructure.Email;

[CollectionDefinition(Name)]
public sealed class MailpitIntegrationTestCollectionDefinition
    : ICollectionFixture<MailpitIntegrationTestFixture>
{
    public const string Name = "Mailpit integration";
}
