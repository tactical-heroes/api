using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.Matchmaking;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.MatchmakingProcessing;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.DependencyInjection;

internal static class SchedulingServiceCollectionExtensions
{
    internal static IServiceCollection AddScheduling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<MatchmakingOptions>, MatchmakingOptionsValidator>();
        services.AddOptions<MatchmakingOptions>()
            .Bind(configuration.GetSection(MatchmakingOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<MatchmakingProcessingOptions>, MatchmakingProcessingOptionsValidator>();
        services.AddOptions<MatchmakingProcessingOptions>()
            .Bind(configuration.GetSection(MatchmakingProcessingOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MatchmakingOptions>>().Value;
            var rules = MatchmakingRules.Create(
                initialRatingGap: options.InitialRatingGap,
                ratingGapStep: options.RatingGapStep,
                expansionInterval: options.ExpansionInterval,
                maximumRatingGap: options.MaximumRatingGap);
            if (rules.IsFailure)
            {
                throw new InvalidOperationException("Matchmaking rating rules are invalid.");
            }

            return rules.Value;
        });
        services.AddHostedService<MatchmakingWorker>();

        return services;
    }
}
