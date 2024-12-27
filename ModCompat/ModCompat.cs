
namespace NetworkTweaks;
internal static class ModCompat
{
    internal const string NetworkGUID = "org.bepinex.plugins.network";
    internal const string ReturnToSenderGUID = "redseiko.valheim.returntosender";
    internal static bool IsNetworkInstalled => IsPluginInstalled(NetworkGUID);
    internal static bool IsReturnToSenderInstalled => IsPluginInstalled(ReturnToSenderGUID);

    internal static bool HasIncompatibleMods()
    {
        bool network = IsNetworkInstalled;
        bool returnToSender = IsReturnToSenderInstalled;    
        if (network)
        {
            Logging.Log.LogWarning($"Incompatible mod: {NetworkGUID} is installed!");
        }
        if (returnToSender)
        {
            Logging.Log.LogWarning($"Incompatible mod: {ReturnToSenderGUID} is installed!");
        }

        return network || returnToSender;
    }

    private static bool IsPluginInstalled(string pluginGUID)
    {
        
        return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(pluginGUID);
    }
}
