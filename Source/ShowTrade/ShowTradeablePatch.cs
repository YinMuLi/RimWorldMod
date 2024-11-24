using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BetterGameLife.Source.ShowTrade
{
    [HarmonyPatch(typeof(Settlement), nameof(Settlement.GetGizmos))]
    internal class ShowTradeablePatch
    {
        /// <summary>
        /// Settlement:定居点 Gizmos：小控件
        /// </summary>
        private static void Postfix(Settlement __instance, ref IEnumerable<Gizmo> __result)
        {
            //Colonists:殖民者 Hostile:敌对
            if (__instance.CanTradeNow && !FactionUtility.HostileTo(__instance.Faction, Faction.OfPlayer))
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
                        float statValue;
                        //基地
                        foreach (Map map in Find.Maps.Where(m => m.ParentFaction == Faction.OfPlayer))
                        {
                            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned.
                            Where(p => p.IsColonist && !p.WorkTagIsDisabled(WorkTags.Social)))
                            {
                                statValue = StatExtension.GetStatValue(pawn, StatDefOf.TradePriceImprovement);
                                if (statValue > num)
                                {
                                    num = statValue;
                                    negotiator = pawn;
                                }
                            }
                        }
                        //远行队
                        foreach (Caravan caravan in Find.WorldObjects.Caravans.Where(c => c.Faction == Faction.OfPlayer))
                        {
                            foreach (Pawn pawn in caravan.PawnsListForReading.
                            Where(p => p.IsColonist && !p.WorkTagIsDisabled(WorkTags.Social)))
                            {
                                statValue = StatExtension.GetStatValue(pawn, StatDefOf.TradePriceImprovement, true, -1);
                                if (statValue > num)
                                {
                                    num = statValue;
                                    negotiator = pawn;
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