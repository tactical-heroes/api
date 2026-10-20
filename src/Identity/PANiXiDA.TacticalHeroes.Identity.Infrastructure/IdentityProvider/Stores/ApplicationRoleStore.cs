using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Stores;

internal sealed class ApplicationRoleStore : RoleStore<
    ApplicationRole,
    IdentityWriteDbContext,
    Guid,
    ApplicationUserRole,
    ApplicationRoleClaim>
{
    public ApplicationRoleStore(IdentityWriteDbContext context, IdentityErrorDescriber describer)
        : base(context, describer)
    {
        AutoSaveChanges = false;
    }

    public override Task<IdentityResult> UpdateAsync(
        ApplicationRole role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (Context.Entry(role).State != EntityState.Added)
        {
            Context.Update(role);
        }

        role.ConcurrencyStamp = Guid.NewGuid().ToString();
        return Task.FromResult(IdentityResult.Success);
    }
}
