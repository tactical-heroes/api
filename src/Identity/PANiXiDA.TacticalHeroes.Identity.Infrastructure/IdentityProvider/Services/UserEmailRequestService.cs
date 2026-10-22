using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.IdentityProvider;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.EmailRequests;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserEmailRequestService(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityProviderOptions> options,
    IAggregateTracker aggregateTracker,
    EmailRequestCooldownStore cooldown,
    TimeProvider timeProvider)
{
    internal async Task<Result> RequestAsync(
        ApplicationUser applicationUser,
        EmailRequestPurpose purpose,
        User? user,
        CancellationToken cancellationToken)
    {
        if (!await cooldown.TryAcquireAsync(
            normalizedEmail: applicationUser.NormalizedEmail!,
            purpose: purpose,
            cancellationToken: cancellationToken))
        {
            return Result.Success();
        }

        if (user is null)
        {
            var userResult = ApplicationUserMapper.ToDomain(user: applicationUser);
            if (userResult.IsFailure)
            {
                return Result.Failure(errors: userResult.Errors);
            }

            user = userResult.Value;
        }

        var token = purpose == EmailRequestPurpose.Confirmation
            ? await userManager.GenerateEmailConfirmationTokenAsync(applicationUser)
            : await userManager.GeneratePasswordResetTokenAsync(applicationUser);
        var lifetime = purpose == EmailRequestPurpose.Confirmation
            ? options.Value.EmailConfirmationTokenLifetime
            : options.Value.AccountRecoveryTokenLifetime;
        var tokenResult = UserActionToken.Create(
            value: token,
            expiresAtUtc: timeProvider.GetUtcNow().Add(lifetime));
        if (tokenResult.IsFailure)
        {
            return Result.Failure(errors: tokenResult.Errors);
        }

        var result = purpose == EmailRequestPurpose.Confirmation
            ? user.RequestEmailConfirmation(tokenResult.Value)
            : user.RequestPasswordReset(tokenResult.Value);
        if (result.IsSuccess)
        {
            aggregateTracker.Track(user);
        }

        return result;
    }
}
