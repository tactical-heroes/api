using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepository(
    IdentityWriteDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IRolesWriteRepository
{
    public async Task<Result<Guid>> AddAsync(
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roleResult = CreateRole(Guid.NewGuid(), name, claims);

        if (roleResult.IsFailure)
        {
            return Result.Failure<Guid>(roleResult.Errors);
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationRole = new ApplicationRole
        {
            Id = roleResult.Value.Id.Value,
            Name = roleResult.Value.Name.Value,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc,
            Claims = CreateClaims(claims)
        };

        var identityResult = await roleManager.CreateAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(identityResult);
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success(applicationRole.Id);
    }

    public async Task<Result> UpdateAsync(
        Guid id,
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken)
    {
        var roleResult = CreateRole(id, name, claims);

        if (roleResult.IsFailure)
        {
            return Result.Failure(roleResult.Errors);
        }

        var applicationRole = await roleManager.Roles
            .Include(role => role.Claims)
            .SingleOrDefaultAsync(role => role.Id == id, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        applicationRole.Name = roleResult.Value.Name.Value;
        applicationRole.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        SyncClaims(applicationRole, claims);

        var identityResult = await roleManager.UpdateAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(identityResult);
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var applicationRole = await roleManager.Roles
            .Include(role => role.Claims)
            .SingleOrDefaultAsync(role => role.Id == id, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        var roleResult = CreateRole(
            applicationRole.Id,
            applicationRole.Name!,
            applicationRole.Claims.Select(claim =>
                new Claim(claim.ClaimType!, claim.ClaimValue!)).ToArray());

        if (roleResult.IsFailure)
        {
            return Result.Failure(roleResult.Errors);
        }

        var identityResult = await roleManager.DeleteAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(identityResult);
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success();
    }

    private static Result<Role> CreateRole(
        Guid id,
        string name,
        IReadOnlyCollection<Claim> claims)
    {
        return Role.Create(
            id,
            name,
            claims
                .Distinct(IdentityClaimComparer.Instance)
                .Select(claim => (claim.Type, claim.Value)));
    }

    private static List<ApplicationRoleClaim> CreateClaims(
        IEnumerable<Claim> claims)
    {
        return
        [
            .. claims
                .Distinct(IdentityClaimComparer.Instance)
                .Select(claim => new ApplicationRoleClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                })
        ];
    }

    private void SyncClaims(
        ApplicationRole applicationRole,
        IReadOnlyCollection<Claim> claims)
    {
        var targetClaims = claims
            .Distinct(IdentityClaimComparer.Instance)
            .ToArray();

        foreach (var currentClaim in applicationRole.Claims.ToArray())
        {
            var claim = new Claim(currentClaim.ClaimType!, currentClaim.ClaimValue!);

            if (targetClaims.Contains(claim, IdentityClaimComparer.Instance))
            {
                continue;
            }

            dbContext.Set<ApplicationRoleClaim>().Remove(currentClaim);
        }

        foreach (var targetClaim in targetClaims)
        {
            if (applicationRole.Claims.Any(currentClaim =>
                    string.Equals(currentClaim.ClaimType, targetClaim.Type, StringComparison.Ordinal) &&
                    string.Equals(currentClaim.ClaimValue, targetClaim.Value, StringComparison.Ordinal)))
            {
                continue;
            }

            applicationRole.Claims.Add(
                new ApplicationRoleClaim
                {
                    ClaimType = targetClaim.Type,
                    ClaimValue = targetClaim.Value
                });
        }
    }

    private static Result RoleNotFound()
    {
        return Result.Failure(Error.NotFound("Role was not found."));
    }
}
