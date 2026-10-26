using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
namespace LandingLag;
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
    public static EnemyAI[] GetAIs() { return RoundManager.Instance.SpawnedEnemies.ToArray(); }
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetPlayerSafeInShip))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        MethodInfo replacement = AccessTools.Method(typeof(Plugin),nameof(GetAIs));
        MethodInfo original = AccessTools.Method(typeof(UnityEngine.Object), nameof(FindObjectOfType), Type.EmptyTypes)?.MakeGenericMethod(typeof(EnemyAI));
        if (original == null || replacement == null)
            return codes;
        for (int i = 0; i < codes.Count; i++)
            if (codes[i].Calls(original))
                codes[i] = new CodeInstruction(OpCodes.Call, replacement);
        return codes;
    }
}
