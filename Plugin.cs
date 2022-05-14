using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Moretar
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        static ManualLogSource logger;
        static bool isSledging = false;

        private void Awake()
        {
            // Plugin startup logic
            logger = Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded");
            Logger.LogInfo($"Patching...");
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Patched");
        }

        [HarmonyPatch(typeof(Trowel), "LoadMe")]
        [HarmonyPostfix]
        static void LoadMe_Prefix(Trowel __instance)
        {
            MethodInfo ammoChangeMethodinfo = AccessTools.Method(typeof(Trowel), "AmmoChange");
            MethodInfo noFinesAmmoChangeMethodinfo = AccessTools.Method(typeof(Trowel), "NoFinesAmmoChange");

            if (ammoChangeMethodinfo.GetParameters().Length == 2)
            {
                ammoChangeMethodinfo.Invoke(__instance, new object[] { 0f, __instance.muckTransform.GetComponent<Renderer>().material });
                noFinesAmmoChangeMethodinfo.Invoke(__instance, new object[] { 0f, __instance.noFinesTransform.GetComponent<Renderer>().material });
            }
            else
            {
                ammoChangeMethodinfo.Invoke(__instance, new object[] { 0f });
                noFinesAmmoChangeMethodinfo.Invoke(__instance, new object[] { 0f });
            }
        }

        [HarmonyPatch(typeof(Trowel), "AmmoChange")]
        [HarmonyPatch(typeof(Trowel), "NoFinesAmmoChange")]
        [HarmonyPostfix]
        static void AmmoChange_Postfix(Trowel __instance)
        {
            __instance.currentAmmo = __instance.maxAmmo - 0.1f;
            __instance.noFinesCurrentAmmo = __instance.maxAmmo - 0.1f;
        }

        [HarmonyPatch(typeof(NoFinesFrame), "AddToNoFines")]
        [HarmonyPrefix]
        static void AddToNoFines_Prefix(ref float amount)
        {
            amount = 1000f;
        }

        [HarmonyPatch(typeof(NoFinesFrame), "SubtractNoFines")]
        [HarmonyPrefix]
        static void SubtractNoFines_Prefix(ref float amount)
        {
            if (isSledging)
            {
                amount = 1000f;
            }
        }

        [HarmonyPatch(typeof(SledgeHammer), "Hit")]
        [HarmonyPrefix]
        static void Hit_Prefix()
        {
            isSledging = true;
        }

        [HarmonyPatch(typeof(SledgeHammer), "Hit")]
        [HarmonyPostfix]
        static void Hit_Postfix()
        {
            isSledging = false;
        }
    }
}
