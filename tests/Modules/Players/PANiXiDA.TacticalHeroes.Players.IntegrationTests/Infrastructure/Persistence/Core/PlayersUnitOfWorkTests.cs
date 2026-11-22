using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.IntegrationTests.Infrastructure.Persistence.Core;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class PlayersUnitOfWorkTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Unit of work should use the Players write context when transaction is started")]
    public async Task BeginTransactionAsync_Should_UsePlayersWriteContext_When_TransactionIsStarted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<PlayersWriteDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext));

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            unitOfWork.HasActiveTransaction.ShouldBeTrue();
            writeContext.Database.CurrentTransaction.ShouldNotBeNull();
            (await writeContext.Database.CanConnectAsync(cancellationToken)).ShouldBeTrue();
        }
        finally
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            await unitOfWork.DisposeTransactionAsync();
        }

        unitOfWork.HasActiveTransaction.ShouldBeFalse();
        writeContext.Database.CurrentTransaction.ShouldBeNull();
    }
}
