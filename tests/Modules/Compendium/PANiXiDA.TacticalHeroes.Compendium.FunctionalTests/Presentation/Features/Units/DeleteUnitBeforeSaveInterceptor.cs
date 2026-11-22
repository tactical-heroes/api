using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units;

internal sealed class DeleteUnitBeforeSaveInterceptor(Guid unitId, IServiceProvider services)
    : SaveChangesInterceptor
{
    public bool UnitDeleted { get; private set; }
    public bool RequestHadTransaction { get; private set; }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken)
    {
        if (UnitDeleted
            || eventData.Context is not CompendiumWriteDbContext context
            || !context.ChangeTracker.Entries<Unit>().Any(entry => entry.Entity.Id.Value == unitId))
        {
            return result;
        }

        RequestHadTransaction = context.Database.CurrentTransaction is not null;
        await using var scope = services.CreateAsyncScope();
        var competingContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
        var id = UnitId.Create(unitId).Value;
        UnitDeleted = await competingContext.Set<Unit>()
            .Where(unit => unit.Id == id)
            .ExecuteDeleteAsync(cancellationToken) == 1;

        return result;
    }
}
