using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Infrastructure.Persistence.Core;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class BuildsUnitOfWorkTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Unit of work should use the Builds write context when transaction is started")]
    public async Task BeginTransactionAsync_Should_UseBuildsWriteContext_When_TransactionIsStarted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<BuildsWriteDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext));

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
