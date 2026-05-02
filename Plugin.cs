using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;

namespace NoLoadLagLethal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Lethal Company.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    Harmony HarInstance;
    public static List<EnemyAI> EnemyAIList = new();
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        HarInstance = Harmony.CreateAndPatchAll(typeof(Plugin));
        Logger.LogInfo($"Applied game patch to round start code.");
    }
    [HarmonyPatch(typeof(StartOfRound), "SetPlayerSafeInShip")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    static void Prefix(StartOfRound __instance, ref bool __runOriginal)
    {
        if (!__runOriginal)
            return;
        __runOriginal = false;
        if (GameNetworkManager.Instance == null || (UnityEngine.Object)(object)GameNetworkManager.Instance.localPlayerController == null)
            return;
        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        if (playerControllerB.isPlayerDead && (UnityEngine.Object)(object)playerControllerB.spectatedPlayerScript != null)
            playerControllerB = playerControllerB.spectatedPlayerScript;
        List<EnemyAI> allEnemies = EnemyAIList;
        if (__instance.hangarDoorsClosed && GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
        {
            for (int i = 0; i < allEnemies.Count; i++)
                allEnemies[i].EnableEnemyMesh(allEnemies[i].isInsidePlayerShip);
            return;
        }
        for (int j = 0; j < allEnemies.Count; j++)
            allEnemies[j].EnableEnemyMesh(true);
    }
    [HarmonyPatch(typeof(EnemyAI), "Start")]
    [HarmonyPriority(Priority.First)]
    [HarmonyPostfix]
    static void PostFix(EnemyAI __instance)
    {
        if (!EnemyAIList.Contains(__instance))
            EnemyAIList.Add(__instance);
    }
    [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    static void Prefix1(EnemyAI __instance)
    {
        EnemyAIList.Remove(__instance);
    }
}
