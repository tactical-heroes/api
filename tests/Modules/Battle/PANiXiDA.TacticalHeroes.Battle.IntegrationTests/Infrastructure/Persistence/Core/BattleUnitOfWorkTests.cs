using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Battle.IntegrationTests.Infrastructure.Persistence.Core;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class BattleUnitOfWorkTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Unit of work should use the Battle write context when transaction is started")]
    public async Task BeginTransactionAsync_Should_UseBattleWriteContext_When_TransactionIsStarted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<BattleWriteDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BattleWriteDbContext));

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
