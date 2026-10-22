using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.EmailRequestCooldown;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.EmailRequests;

public sealed class EmailRequestCooldownStore(
    IdentityWriteDbContext dbContext,
    IOptions<EmailRequestCooldownOptions> options)
{
    internal async Task<bool> TryAcquireAsync(
        string normalizedEmail,
        EmailRequestPurpose purpose,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Email cooldown must share the Identity command transaction and outbox.");
        }

        var interval = purpose == EmailRequestPurpose.Confirmation
            ? options.Value.Confirmation
            : options.Value.PasswordReset;
        var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO identity.email_request_cooldowns (normalized_email, purpose, expires_at)
            VALUES ({normalizedEmail}, {(int)purpose}, clock_timestamp() + {interval})
            ON CONFLICT (normalized_email, purpose) DO UPDATE
            SET expires_at = clock_timestamp() + {interval}
            WHERE email_request_cooldowns.expires_at <= clock_timestamp()
            """,
            cancellationToken);

        return affected == 1;
    }
}
