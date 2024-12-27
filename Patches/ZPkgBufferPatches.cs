using System.Collections.Generic;
using HarmonyLib;

namespace NetworkTweaks.Patches;

/// <summary>
///     Buffers data while connecting and then parses it.
/// </summary>
[HarmonyPatch]
internal static class ZPkgBufferPatches
{
    private static readonly List<ZPackage> zpkgBuffer = [];

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    private static void StartBuffering(ZNet __instance, ZNetPeer peer)
    {
        if (!__instance || __instance.IsServer())
        {
            return;
        }

        // Replace calls to RPC_ZDOData with calls to this delegate.
        peer.m_rpc.Register<ZPackage>("ZDOData", (_, package) => zpkgBuffer.Add(package));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Shutdown))]
    private static void ClearBufferOnShutdown()
    {
        zpkgBuffer.Clear();
    }


    /// <summary>
    ///     Read packages that accumulated in buffer while connecting
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="peer"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.AddPeer))]
    private static void ParseBufferedZPackages(ZDOMan __instance, ZNetPeer netPeer)
    {
        foreach (ZPackage pkg in zpkgBuffer)
        {
            __instance.RPC_ZDOData(netPeer.m_rpc, pkg);
        }
        zpkgBuffer.Clear();
    }
}
