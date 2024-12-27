using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using HarmonyLib;
using Steamworks;

namespace NetworkTweaks.Patches;


[HarmonyPatch]
internal static class SteamDataTransferPatches
{
    private const int VanillaTransferRate = 153600; // bytes
    private const int MaxTransferRate = 50 * (int)1e6; // 50 MB
    private const int BufferSize = 100 * (int)1e6 ; // 100 MB

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ZSteamSocket), nameof(ZSteamSocket.RegisterGlobalCallbacks))]
    private static IEnumerable<CodeInstruction> ChangeSendingLimitTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        // targeting GCHandle gcHandle = GCHandle.Alloc(153600, GCHandleType.Pinned);
        CodeMatch[] codeMatches = { new CodeMatch(OpCodes.Ldc_I4, VanillaTransferRate) };
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, codeMatches)
            .ThrowIfInvalid("Could not patch ZSteamSocket.RegisterGlobalCallbacks()!")
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, MaxTransferRate))
            .InstructionEnumeration();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZSteamSocket), nameof(ZSteamSocket.RegisterGlobalCallbacks))]
    private static void IncreaseSendBufferSize()
    {
        if (CSteamAPIContext.GetSteamClient() == IntPtr.Zero)
        {
            return;
        }
        GCHandle handle = GCHandle.Alloc(BufferSize, GCHandleType.Pinned);
        SteamNetworkingUtils.SetConfigValue(
            ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize, 
            ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, 
            IntPtr.Zero, 
            ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32, 
            handle.AddrOfPinnedObject()
        );
        handle.Free();
    }
}
