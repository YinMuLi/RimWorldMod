using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using YinMu.Source.ShowTrade;

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

        /// <summary>
        /// Settlement:定居点 Gizmos：小控件
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Settlement), nameof(Settlement.GetGizmos))]
        private static void GetGizmos(Settlement __instance, ref IEnumerable<Gizmo> __result)
        {
            //Colonists:殖民者
            if (__instance.CanTradeNow && !__instance.Faction.def.permanentEnemy)
            {
                //__result.AddItem没有用
                List<Gizmo> list = __result.ToList();
                list.Add(new Command_Action
                {
                    defaultLabel = "CommandShowSettlementGoods".Translate(),
                    defaultDesc = "CommandShowSettlementGoodsDesc".Translate(),
                    icon = Settlement.ShowSellableItemsCommand,
                    action = () =>
                    {
                        //寻找在此部落协商最成功的小人
                        Pawn negotiator = null;
                        float num = 0f;
                        //基地
                        foreach (Map map in Find.Maps.Where(m => m.ParentFaction == Faction.OfPlayer))
                        {
                            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
                            {
                                if (pawn.IsColonist && !pawn.WorkTagIsDisabled(WorkTags.Social))
                                {
                                    float statValue = StatExtension.GetStatValue(pawn, StatDefOf.TradePriceImprovement, true, -1);
                                    if (statValue > num)
                                    {
                                        num = statValue;
                                        negotiator = pawn;
                                    }
                                }
                            }
                        }
                        //远行队
                        foreach (Caravan caravan in Find.WorldObjects.Caravans.Where(c => c.Faction == Faction.OfPlayer))
                        {
                            foreach (Pawn pawn in caravan.PawnsListForReading)
                            {
                                if (pawn.IsColonist && !pawn.WorkTagIsDisabled(WorkTags.Social))
                                {
                                    float statValue2 = StatExtension.GetStatValue(pawn, StatDefOf.TradePriceImprovement, true, -1);
                                    if (statValue2 > num)
                                    {
                                        num = statValue2;
                                        negotiator = pawn;
                                    }
                                }
                            }
                        }
                        Find.WindowStack.Add(new Dialog_TradeableGoods(__instance, negotiator));
                        RoyalTitleDef titleRequiredToTrade = __instance.TraderKind.TitleRequiredToTrade;
                        if (titleRequiredToTrade != null)
                        {
                            TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradingRequiresPermit, titleRequiredToTrade.GetLabelCapForBothGenders());
                        }
                    }
                });
                __result = list;
            }
        }
    }
}