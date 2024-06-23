using HarmonyLib;
using RimWorld;
using System;
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
                    case DestroyMode.Deconstruct://拆除（不包括移除地板）
                    case DestroyMode.FailConstruction://建造失败
                    case DestroyMode.KillFinalize://完全摧毁？？
                        //用的环世界源代码写法
                        __result = (int count) => GenMath.RoundRandom((float)count * 1f);
                        break;
                }
            }
        }

        /// <summary>
        /// 植物不会得枯萎病,虽然会有黄色通知
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

            if (ModSettings.Instance.betterSpoils && dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(__instance.pawn))
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
                return false;
            }
            return true;
        }

        /// <summary>
        /// 当尸体开始腐烂，衣服打上“亡者”标签
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Corpse), nameof(Corpse.RotStageChanged))]
        private static void RotStageChanged(Corpse __instance)
        {
            /**
             * Corpse:尸体 Rot
             * :腐烂
             */
            if (ModSettings.Instance.betterSpoils && __instance.InnerPawn.apparel != null)
            {
                foreach (var apparel in __instance.InnerPawn.apparel.WornApparel)
                {
                    apparel.Notify_PawnKilled();
                }
            }
        }

        #endregion 尸体腐烂小人穿的衣服才会有“已亡”

        /// <summary>
        /// 小人禁止生成亲戚
        /// </summary>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PawnRelationWorker), nameof(PawnRelationWorker.BaseGenerationChanceFactor))]
        private static bool BaseGenerationChanceFactor(ref float __result)
        {
            __result = 0f;
            return false;
        }

        private static readonly float with = 24f;

        /// <summary>
        /// 商队贸易界面显示科技蓝图需求情况
        /// </summary>
        /// <param name="trad"></param>
        /// <param name="rect"></param>
        /// <param name="curX"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TransferableUIUtility), nameof(TransferableUIUtility.DoExtraIcons))]
        private static void DoExtraIcons(Transferable trad, Rect rect, ref float curX)
        {
            var techThing = trad.AnyThing.TryGetComp<CompTechprint>();
            if (techThing != null)
            {
                Rect rect1 = new Rect(curX - with, (rect.height - with) / 2, with, with);
                var res = techThing.Props.project.TechprintCount - techThing.Props.project.TechprintsApplied;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect1, res.ToString().Colorize(Color.cyan));
                if (Mouse.IsOver(rect1))
                {
                    Widgets.DrawHighlight(rect1);
                }
                curX -= with;
            }
        }

        /// <summary>
        /// 自动砍伐树桩
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="treeDestructionMode"></param>
        /// <param name="__result"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Plant), nameof(Plant.TrySpawnStump))]
        private static void TrySpawnStump(Plant __instance, PlantDestructionMode treeDestructionMode, Thing __result)
        {
            if (__result != null && treeDestructionMode == PlantDestructionMode.Chop &&
                ModSettings.Instance.autoChopStumps
                )
            {
                __instance.Map.designationManager.AddDesignation(new Designation(__result, DesignationDefOf.HarvestPlant));
            }
        }
    }
}