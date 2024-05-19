using HarmonyLib;
using RimWorld;

using System;
using Verse;

namespace YinMu.Source
{
    [Harmony]
    internal class SimplePatch
    {
        /// <summary>
        /// 摧毁建筑返还所有材料
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GenLeaving), "GetBuildingResourcesLeaveCalculator")]
        private static void GetBuildingResourcesLeaveCalculator(Thing destroyedThing, DestroyMode mode, ref Func<int, int> __result)
        {
            if (mode == DestroyMode.Deconstruct && GenLeaving.CanBuildingLeaveResources(destroyedThing, mode))
            {
                __result = (int count) => count;
            }
        }
    }
}