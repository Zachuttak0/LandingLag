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
    static bool Prefix(StartOfRound __instance)
    {
        if (GameNetworkManager.Instance == null || (UnityEngine.Object)(object)GameNetworkManager.Instance.localPlayerController == null)
            return false;
        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        if (playerControllerB.isPlayerDead && (UnityEngine.Object)(object)playerControllerB.spectatedPlayerScript != null)
            playerControllerB = playerControllerB.spectatedPlayerScript;
        List<EnemyAI> allEnemies = EnemyAI._allEnemies;
        if (__instance.hangarDoorsClosed && GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
        {
            for (int i = 0; i < allEnemies.Count; i++)
                allEnemies[i].EnableEnemyMesh(allEnemies[i].isInsidePlayerShip);
            return false;
        }
        for (int j = 0; j < allEnemies.Count; j++)
            allEnemies[j].EnableEnemyMesh(true);
        return false;
    }
}
