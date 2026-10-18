using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepository(
    IdentityWriteDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IRolesRepository
{
    public async Task<Result<Guid>> AddAsync(
        Role role,
        CancellationToken cancellationToken)
    {
        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        await LockRolesAsync(cancellationToken);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationRole = ApplicationRoleMapper.ToDbModel(
            role: role,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        applicationRole.IsDefault = false;
        var identityResult = await roleManager.CreateAsync(role: applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(result: identityResult);
        }

        await SetDefaultAsync(applicationRole, role.IsDefault, cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        aggregateTracker.Track(role);

        return Result.Success(value: applicationRole.Id);
    }

    public async Task<Result> UpdateAsync(
        Role role,
        CancellationToken cancellationToken)
    {
        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        await LockRolesAsync(cancellationToken);

        var applicationRole = await roleManager.Roles
            .Include(item => item.Claims)
            .SingleOrDefaultAsync(item => item.Id == role.Id.Value, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        if (applicationRole.IsDefault && !role.IsDefault)
        {
            return DefaultRoleConflict();
        }

        var wasDefault = applicationRole.IsDefault;
        ApplicationRoleMapper.MapToDbModel(
            role: role,
            dbModel: applicationRole,
            updatedAt: timeProvider.GetUtcNow().UtcDateTime);
        applicationRole.IsDefault = wasDefault;
        SyncClaims(
            applicationRole: applicationRole,
            role: role);

        var identityResult = await roleManager.UpdateAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        await SetDefaultAsync(applicationRole, role.IsDefault, cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        aggregateTracker.Track(role);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        await LockRolesAsync(cancellationToken);

        var applicationRole = await roleManager.Roles
            .Include(role => role.Claims)
            .SingleOrDefaultAsync(role => role.Id == id, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        if (applicationRole.IsDefault)
        {
            return DefaultRoleConflict();
        }

        var roleResult = ApplicationRoleMapper.ToDomain(role: applicationRole);

        if (roleResult.IsFailure)
        {
            return Result.Failure(errors: roleResult.Errors);
        }

        var identityResult = await roleManager.DeleteAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success();
    }

    private Task LockRolesAsync(CancellationToken cancellationToken)
    {
        return dbContext.Database.ExecuteSqlRawAsync(
            "LOCK TABLE identity.asp_net_roles IN SHARE ROW EXCLUSIVE MODE", cancellationToken);
    }

    private async Task SetDefaultAsync(
        ApplicationRole role,
        bool isDefault,
        CancellationToken cancellationToken)
    {
        if (!isDefault || role.IsDefault)
        {
            return;
        }

        var previousDefaults = await dbContext.Set<ApplicationRole>()
            .Where(item => item.IsDefault && item.Id != role.Id)
            .ToArrayAsync(cancellationToken);
        foreach (var previous in previousDefaults)
        {
            previous.IsDefault = false;
            previous.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        role.IsDefault = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Result DefaultRoleConflict()
    {
        return Result.Failure(error: Error.Conflict(message: "Select another default role first."));
    }

    private static Result RoleNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "Role was not found."));
    }

    private void SyncClaims(
        ApplicationRole applicationRole,
        Role role)
    {
        var targetClaims = ApplicationRoleMapper.ToClaimDbModels(claims: role.Claims);

        foreach (var currentClaim in applicationRole.Claims.ToArray())
        {
            if (
                targetClaims.Any(
                    targetClaim =>
                        string.Equals(targetClaim.ClaimType, currentClaim.ClaimType, StringComparison.Ordinal)
                            && string.Equals(
                                targetClaim.ClaimValue,
                                currentClaim.ClaimValue,
                                StringComparison.Ordinal)))
            {
                continue;
            }

            dbContext.Set<ApplicationRoleClaim>().Remove(currentClaim);
        }

        foreach (var targetClaim in targetClaims)
        {
            if (
                applicationRole.Claims.Any(
                    currentClaim =>
                        string.Equals(currentClaim.ClaimType, targetClaim.ClaimType, StringComparison.Ordinal)
                            && string.Equals(
                                currentClaim.ClaimValue,
                                targetClaim.ClaimValue,
                                StringComparison.Ordinal)))
            {
                continue;
            }

            applicationRole.Claims.Add(item: targetClaim);
        }
    }
}
