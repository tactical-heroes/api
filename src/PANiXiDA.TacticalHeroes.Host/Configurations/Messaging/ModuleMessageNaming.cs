using Wolverine.Attributes;
using Wolverine.Util;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Messaging;

internal sealed class ModuleMessageNaming : IMessageTypeNaming
{
    private const string ApplicationPrefix = "PANiXiDA.TacticalHeroes.";
    private const string ModulePrefix = ApplicationPrefix + "Modules.";

    public bool TryDetermineName(Type messageType, out string messageTypeName)
    {
        // Wolverine envelopes persisted before the module move use the original type name.
        if (messageType.FullName is { } fullName
            && fullName.StartsWith(ModulePrefix, StringComparison.Ordinal)
            && !messageType.IsGenericType
            && !messageType.IsNested
            && !messageType.IsDefined(typeof(MessageIdentityAttribute), inherit: true))
        {
            messageTypeName = ApplicationPrefix + fullName[ModulePrefix.Length..];
            return true;
        }

        messageTypeName = string.Empty;
        return false;
    }
}
