using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Deferred.CleanupVersion;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Recurring.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Recurring.ReconcileDeletions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Deferred.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Recurring.Expire;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Recurring.Process;

using Wolverine.Util;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Messaging;

internal sealed class ScheduledCommandNaming : IMessageTypeNaming
{
    private const string FileManagerApplication = "PANiXiDA.TacticalHeroes.FileManager.Application.";
    private const string MatchmakingApplication = "PANiXiDA.TacticalHeroes.Matchmaking.Application.";

    // Keep the identities already stored in Wolverine's scheduled envelopes.
    private static readonly Dictionary<Type, string> s_messageNames = new()
    {
        [typeof(CleanupFileVersionCommand)] =
            FileManagerApplication + "Files.CleanupVersion." + nameof(CleanupFileVersionCommand),
        [typeof(ReconcileFileUploadsCommand)] =
            FileManagerApplication + "Files.ReconcileUploads." + nameof(ReconcileFileUploadsCommand),
        [typeof(ReconcileFolderDeletionsCommand)] =
            FileManagerApplication + "Folders.ReconcileDeletions." + nameof(ReconcileFolderDeletionsCommand),
        [typeof(ConfirmMatchCommand)] =
            MatchmakingApplication + "MatchSearches.ConfirmMatch." + nameof(ConfirmMatchCommand),
        [typeof(ExpireMatchSearchesCommand)] =
            MatchmakingApplication + "MatchSearches.Expire." + nameof(ExpireMatchSearchesCommand),
        [typeof(ProcessMatchmakingCommand)] =
            MatchmakingApplication + "MatchSearches.Process." + nameof(ProcessMatchmakingCommand)
    };

    public bool TryDetermineName(Type messageType, out string messageTypeName)
    {
        var found = s_messageNames.TryGetValue(messageType, out var storedName);
        messageTypeName = storedName ?? string.Empty;
        return found;
    }
}
