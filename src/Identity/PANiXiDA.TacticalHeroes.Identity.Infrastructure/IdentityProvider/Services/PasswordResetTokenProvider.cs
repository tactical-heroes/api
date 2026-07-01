using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

internal sealed class PasswordResetTokenProvider(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<PasswordResetTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<ApplicationUser>> logger)
    : DataProtectorTokenProvider<ApplicationUser>(
        dataProtectionProvider,
        Microsoft.Extensions.Options.Options.Create(new DataProtectionTokenProviderOptions
        {
            Name = options.Value.Name,
            TokenLifespan = options.Value.TokenLifespan
        }),
        logger);
