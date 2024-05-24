using HarmonyLib;
using HugsLib.Utils;
using RimWorld;

using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace YinMu.Source
{
    /// <summary>
    /// 类上一定要加[Harmony]
    /// </summary>
    [Harmony]
    internal class SimplePatch
    {
        /// <summary>
        /// 建筑返还所有材料
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GenLeaving), "GetBuildingResourcesLeaveCalculator")]
        private static void GetBuildingResourcesLeaveCalculator(Thing destroyedThing, DestroyMode mode, ref Func<int, int> __result)
        {
            if (GenLeaving.CanBuildingLeaveResources(destroyedThing, mode))
            {
                switch (mode)
                {
                    case DestroyMode.Deconstruct://拆除
                    case DestroyMode.FailConstruction://建造失败
                    case DestroyMode.KillFinalize://完全摧毁
                        //用的环世界源代码写法
                        __result = (int count) => GenMath.RoundRandom((float)count * 1f);
                        break;
                }
            }
        }

        /// <summary>
        /// 植物不会得枯萎病
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Plant), nameof(Plant.CropBlighted))]
        private static bool CropBlighted() => false;

        #region 尸体腐烂小人穿的衣服才会有“已亡”

        /// <summary>
        /// 去除击杀小人穿着衣物上的死亡标签
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_PawnKilled))]
        private static bool Notify_PawnKilled(Pawn_ApparelTracker __instance, DamageInfo? dinfo)
        {
            /**
             * Apparel:衣物 worn:穿
             */
            if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(__instance.pawn))
            {
                var wornApparel = __instance.WornApparel;
                for (int i = 0; i < wornApparel.Count; i++)
                {
                    if (wornApparel[i].def.useHitPoints)
                    {
                        int num = Mathf.RoundToInt((float)wornApparel[i].HitPoints * Rand.Range(0.15f, 0.4f));
                        wornApparel[i].TakeDamage(new DamageInfo(dinfo.Value.Def, num));
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 当尸体开始腐烂，衣服打上“亡者”标签
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Corpse), nameof(Corpse.RotStageChanged))]
        private static void RotStageChanged(Corpse __instance)
        {
            /**
             * Corpse:尸体 Rot:腐烂
             */
            foreach (var apparel in __instance.InnerPawn.apparel.WornApparel)
            {
                apparel.Notify_PawnKilled();
            }
        }

        #endregion 尸体腐烂小人穿的衣服才会有“已亡”
    }
}