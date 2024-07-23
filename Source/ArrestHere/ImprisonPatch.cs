using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace YinMu.Source.ArrestHere
{
    //[Harmony]
    //internal class ImprisonPatch
    //{
    //    [HarmonyPostfix]
    //    [HarmonyPatch(typeof(JobGiver_PrisonerEscape), "ShouldStartEscaping")]
    //    public static void Postfix(ref bool __result, Pawn pawn)
    //    {
    //        if (__result)
    //        {
    //            //就地逮捕一天之内不逃跑
    //            var comp = pawn.TryGetComp<Comp_Imprisonment>();
    //            if (comp.lastTryingEscapeTick == 0)
    //            {
    //                comp.lastTryingEscapeTick = Find.TickManager.TicksGame;
    //                __result = false;
    //            }
    //            else if (Find.TickManager.TicksGame < comp.lastTryingEscapeTick + GenDate.TicksPerDay)
    //            {
    //                __result = false;
    //            }
    //            else
    //            {
    //                comp.lastTryingEscapeTick = 0;
    //            }
    //        }
    //    }

    //    [HarmonyPostfix]
    //    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    //    private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
    //    {
    //        if (pawn == null) return;
    //        var pos = IntVec3.FromVector3(clickPos);
    //        //Manipulation:操作 Capable:有能力 Capacity：能力
    //        if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
    //        {
    //            //你控制的小人有操作的能力
    //            //victim:受害者
    //            foreach (Pawn victim in pos.GetThingList(pawn.Map).
    //                Where(t => t is Pawn p && p.RaceProps.Humanlike))
    //            {
    //                if (pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, ignoreOtherReservations: true))
    //                {
    //                    //你的小人能到达受害人的位置
    //                    if (CanArrested(victim) || PrisonerIsEscaping(victim))
    //                    {
    //                        //非本殖民者倒地或者囚犯逃跑
    //                        TaggedString label = "ImprisonInPlace".Translate(victim.LabelCap);
    //                        if (victim.Faction != null && victim.Faction != Faction.OfPlayer && !victim.Faction.Hidden && !victim.Faction.HostileTo(Faction.OfPlayer) && !victim.IsPrisonerOfColony)
    //                        {
    //                            label += ": " + "AngersFaction".Translate().CapitalizeFirst();
    //                        }

    //                        //受害人倒地或者逃脱
    //                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, () =>
    //                        {
    //                            Job job = JobMaker.MakeJob(ModDefOf.ImprisonInPlace, victim);
    //                            job.count = 1;
    //                            pawn.jobs.TryTakeOrderedJob(job);
    //                            if (victim.Faction != null && victim.Faction != Faction.OfPlayer && !victim.Faction.Hidden && !victim.Faction.HostileTo(Faction.OfPlayer) && !victim.IsPrisonerOfColony)
    //                            {
    //                                Messages.Message("MessageCapturingWillAngerFaction".Translate(victim.Named("PAWN")).AdjustedFor(victim), victim, MessageTypeDefOf.CautionInput, historical: false);
    //                            }
    //                        }, MenuOptionPriority.Default, null, victim), pawn, victim, "ReservedBy"));
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    //囚犯逃跑
    //    private static bool PrisonerIsEscaping(Pawn pawn)
    //    {
    //        return pawn.CurJobDef == JobDefOf.Goto
    //            && pawn.CurJob.exitMapOnArrival
    //            && pawn.mindState.lastJobTag == JobTag.Escaping;
    //    }

    //    //可以就地逮捕的小人
    //    private static bool CanArrested(Pawn pawn)
    //    {
    //        return pawn.Downed && !pawn.IsFactionOfPlayer();
    //    }
    //}
}