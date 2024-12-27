using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Utils;
using Configs;
using Logging;


namespace NetworkTweaks;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid, Jotunn.Main.Version)]
[BepInDependency(ModCompat.NetworkGUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompat.ReturnToSenderGUID, BepInDependency.DependencyFlags.SoftDependency)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
internal sealed class NetworkTweaks : BaseUnityPlugin
{
    public const string PluginName = "NetworkTweaks";
    internal const string Author = "Searica";
    public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
    public const string PluginVersion = "0.1.3";

    internal static NetworkTweaks Instance;
    internal static ConfigFile ConfigFile;

    // Global settings
    internal const string GlobalSection = "Global";
    internal ConfigEntry<int> PeersPerUpdate;

    public void Awake()
    {
        Instance = this;
        ConfigFile = Config;
        Log.Init(Logger);

        Config.Init(PluginGUID, false);
        SetUpConfigEntries();
        Config.Save();
        Config.SaveOnConfigSet = true;

        if (ModCompat.HasIncompatibleMods())
        {
            Log.LogWarning($"Incompatible mods detected, {PluginName} will not load!");
            return;
        }

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        Game.isModded = true;

        // Re-initialization after reloading config and don't save since file was just reloaded
        Config.SetupWatcher();
    }

    internal void SetUpConfigEntries()
    {
        PeersPerUpdate = Config.BindConfigInOrder(
            GlobalSection,
            "Peers Per Update",
            10,
            "Number of peers to sync data to each update tick. Vanilla default is 1."
            + " The higher this is the more data needs to be transferred each update tick.",
            new AcceptableValueRange<int>(1, 50),
            synced: true
        );
    }


    public void OnDestroy()
    {
        Config.Save();
    }

}
