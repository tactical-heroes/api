using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Stores;

internal sealed class ApplicationUserStore : UserStore<
    ApplicationUser,
    ApplicationRole,
    IdentityWriteDbContext,
    Guid,
    ApplicationUserClaim,
    ApplicationUserRole,
    ApplicationUserLogin,
    IdentityUserToken<Guid>,
    ApplicationRoleClaim>
{
    public ApplicationUserStore(IdentityWriteDbContext context, IdentityErrorDescriber describer)
        : base(context, describer)
    {
        AutoSaveChanges = false;
    }

    public override Task<IdentityResult> UpdateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (Context.Entry(user).State != EntityState.Added)
        {
            Context.Update(user);
        }

        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        return Task.FromResult(IdentityResult.Success);
    }
}
