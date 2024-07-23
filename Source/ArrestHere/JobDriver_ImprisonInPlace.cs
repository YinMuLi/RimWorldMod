using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace YinMu.Source.ArrestHere
{
    //internal class JobDriver_ImprisonInPlace : JobDriver
    //{
    //    private const int ImprisonDurationTicks = 120;
    //    private Pawn prisoner => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    //    //Reservations:预定 Toil:辛苦工作
    //    public override bool TryMakePreToilReservations(bool errorOnFailed)
    //    {
    //        return pawn.Reserve(prisoner, job, errorOnFailed: errorOnFailed);
    //    }

    //    protected override IEnumerable<Toil> MakeNewToils()
    //    {
    //        //Aggro:仇恨，暴力行为 Mental：心里精神 Hostile：敌对的
    //        //FailOn...应该是终止任务条件
    //        this.FailOnDespawnedOrNull(TargetIndex.A);
    //        this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
    //        //1.移动到目的地
    //        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
    //            .FailOnDespawnedNullOrForbidden(TargetIndex.A)
    //            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
    //        //2.逮捕任务
    //        Toil arrestToil = new Toil();
    //        arrestToil.initAction = delegate
    //        {
    //            if (job.def.makeTargetPrisoner)
    //            {
    //                //通知即将逮捕的囚犯派系，囚犯被玩家逮捕
    //                prisoner.GetLord()?.Notify_PawnAttemptArrested(prisoner);
    //                //Clamor：叫嚷
    //                GenClamor.DoClamor(prisoner, 10f, ClamorDefOf.Harm);
    //                if (!prisoner.IsPrisoner)
    //                {
    //                    //逮捕
    //                    QuestUtility.SendQuestTargetSignals(prisoner.questTags, "Arrested", prisoner.Named("SUBJECT"));
    //                }
    //                if (!prisoner.CheckAcceptArrest(pawn))
    //                {
    //                    //Incompletable：不可完成的
    //                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
    //                }
    //                else if (!prisoner.Downed)
    //                {
    //                    //逮捕成功
    //                    prisoner.jobs.StopAll();
    //                    prisoner.pather.StopDead();
    //                    prisoner.stances.stunner.StunFor(ImprisonDurationTicks, pawn);
    //                }
    //            }
    //        };
    //        yield return arrestToil;
    //        //3.进度条
    //        Toil progressToil = Toils_General.Wait(ImprisonDurationTicks);
    //        progressToil.WithProgressBarToilDelay(TargetIndex.A, true);
    //        yield return progressToil;
    //        Toils_General.Do(CheckMakeTakeeGuest);
    //        Toil toil = new Toil();
    //        toil.initAction = delegate
    //        {
    //            CheckMakeTakeePrisoner();
    //            if (prisoner.playerSettings == null)
    //            {
    //                prisoner.playerSettings = new Pawn_PlayerSettings(prisoner);
    //            }
    //        };
    //        yield return toil;
    //    }

    //    private void CheckMakeTakeePrisoner()
    //    {
    //        if (job.def.makeTargetPrisoner)
    //        {
    //            if (prisoner.guest.Released)
    //            {
    //                prisoner.guest.Released = false;
    //                prisoner.guest.SetNoInteraction();
    //                GenGuest.RemoveHealthyPrisonerReleasedThoughts(prisoner);
    //            }
    //            if (!prisoner.IsPrisonerOfColony)
    //            {
    //                prisoner.guest.CapturedBy(Faction.OfPlayer, pawn);
    //            }
    //            var comp = pawn.TryGetComp<Comp_Imprisonment>();
    //            comp.lastTryingEscapeTick = 0;
    //        }
    //    }

    //    private void CheckMakeTakeeGuest()
    //    {
    //        if (!job.def.makeTargetPrisoner && prisoner.Faction != Faction.OfPlayer && prisoner.HostFaction != Faction.OfPlayer && prisoner.guest != null && !prisoner.IsWildMan())
    //        {
    //            prisoner.guest.SetGuestStatus(Faction.OfPlayer);
    //        }
    //    }
    //}
}