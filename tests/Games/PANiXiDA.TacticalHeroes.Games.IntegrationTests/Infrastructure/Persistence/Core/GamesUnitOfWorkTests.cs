using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Games.IntegrationTests.Infrastructure.Persistence.Core;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class GamesUnitOfWorkTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Unit of work should use the Games write context when transaction is started")]
    public async Task BeginTransactionAsync_Should_UseGamesWriteContext_When_TransactionIsStarted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<GamesWriteDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(GamesWriteDbContext));

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
