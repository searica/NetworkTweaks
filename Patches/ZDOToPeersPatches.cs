using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace NetworkTweaks.Patches;


/// <summary>
///     Replaces call to ZDOMan.SendZDOToPeers2 with a call to a custom SendZDOToPeers method
/// </summary>
[HarmonyPatch]
internal static class ZDOToPeersPatches
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.Update))]
    private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatch match = new (OpCodes.Call, AccessTools.Method(typeof(ZDOMan), nameof(ZDOMan.SendZDOToPeers2)));
        return new CodeMatcher(instructions)
            .Start()
            .MatchStartForward(match)
            .ThrowIfInvalid($"Could not patch ZDOMan.Update()! (send-zdo-to-peers)")
            .SetOperandAndAdvance(AccessTools.Method(typeof(ZDOToPeersPatches), nameof(SendZDOToPeers)))
            .InstructionEnumeration();
    }

    private static void SendZDOToPeers(ZDOMan zdoManager, float dt)
    {
        int count = zdoManager.m_peers.Count;
        if (count <= 0)
        {
            return;
        }
        
        zdoManager.m_sendTimer += dt;
        if (zdoManager.m_sendTimer < 0.05f)
        {
            return;
        }

        zdoManager.m_sendTimer = 0f;
        List<ZDOMan.ZDOPeer> peers = zdoManager.m_peers;
        int currentPeer = Math.Max(zdoManager.m_nextSendPeer, 0);
        int stopAtPeer = Math.Min(currentPeer + NetworkTweaks.Instance.PeersPerUpdate.Value, count);
        for (int i = currentPeer; i < stopAtPeer; i++)
        {
            zdoManager.SendZDOs(peers[i], flush: false);
        }

        // reset nextSendPeer if end of peer list was reached.
        zdoManager.m_nextSendPeer = stopAtPeer < count ? stopAtPeer : 0;
    }
}

