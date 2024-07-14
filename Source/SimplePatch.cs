using HarmonyLib;
using HugsLib.Utils;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Verse.DamageWorker;

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

        #region 尸体腐烂小人穿的衣服才会有“已亡”

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Apparel), nameof(Apparel.Notify_PawnKilled))]
        private static void Notify_PawnKilled(Apparel __instance)
        {
            if (ModEntry.Instance.Handles.betterSpoils)
            {
                __instance.WornByCorpse = false;
            }
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
            if (ModEntry.Instance.Handles.betterSpoils && __instance.InnerPawn.apparel != null)
            {
                var apparels = (ThingOwner<Apparel>)__instance.InnerPawn.apparel.GetDirectlyHeldThings();
                for (int i = 0; i < apparels.Count; i++)
                {
                    apparels[i].WornByCorpse = true;
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
                ModEntry.Instance.Handles.autoChopStumps)
            {
                __instance.Map.designationManager.AddDesignation(new Designation(__result, DesignationDefOf.HarvestPlant));
            }
        }

        #region 显示伤害

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
        private static void TakeDamage(Thing __instance, DamageInfo dinfo, DamageResult __result)
        {
            float damage = __result.totalDamageDealt;
            if (damage > 0.01f && __instance.Map != null &&
                ShouldDisplayDamageInfo(__instance, dinfo.Instigator))
            {
                EmitDamageMote(damage, __instance.Map, __instance.DrawPos);
            }
        }

        private static bool ShouldDisplayDamageInfo(Thing target, Thing instigator)
        {
            //instigator:煽动者
            //TODO:伤害的显示条件
            if (target is Pawn && instigator is Pawn)
            {
                //pawn：小人或动物
                return true;
            }
            return false;
        }

        private static void EmitDamageMote(float damage, Map map, Vector3 pos)
        {
            Color color = Color.white;
            //Determine colour
            if (damage >= 90f)
                color = Color.cyan;
            else if (damage >= 70f)
                color = Color.magenta;
            else if (damage >= 50f)
                color = Color.red;
            else if (damage >= 30f)
                color = Color.Lerp(Color.red, Color.yellow, 0.5f);//orange
            else if (damage >= 10f)
                color = Color.yellow;

            MoteMaker.ThrowText(pos, map, damage.ToString("F0"), color, 2.2f);
        }

        #endregion 显示伤害

        #region 机械师无视距离控制机器

        //现在：机械师只是个培育机械的后勤人员了
        //Mechanitor:机械控制器
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_MechanitorTracker), nameof(Pawn_MechanitorTracker.CanCommandTo))]
        private static bool CanCommandTo(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_MechanitorTracker), nameof(Pawn_MechanitorTracker.DrawCommandRadius))]
        private static bool DrawCommandRadius() => false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.InMechanitorCommandRange))]
        private static bool InMechanitorCommandRange(ref bool __result)
        {
            __result = true;
            return false;
        }

        //mechs:机械
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_MechanitorTracker),
            nameof(Pawn_MechanitorTracker.CanControlMechs), MethodType.Getter)]
        private static void CanControlMechs(Pawn_MechanitorTracker __instance, ref AcceptanceReport __result)
        {
            if (!__result)
            {
                __result |= __instance.Pawn.IsCaravanMember();
            }
        }

        #endregion 机械师无视距离控制机器

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Translator), "PseudoTranslated")]
        private static bool PseudoTranslated(string original, ref string __result)
        {
            __result = original;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.DropAndForbidEverything))]
        private static bool DropAndForbidEverything(Pawn __instance)
        {
            if (__instance.Dead || __instance.Downed)
            {
                return false;
            }
            return true;
        }

        #region 修改初始界面

        //隐藏左侧翻译框（很感激翻译人员的！！）
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoTranslationInfoRect))]
        private static bool DoTranslationInfoRect() => false;

        //去除左下方DLC的信息
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoExpansionIcons))]
        private static bool DoExpansionIcons() => false;

        #endregion 修改初始界面

        //在允许活动区标签上ALT键+左/右键点击，快捷更改活动区域
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PawnColumnWorker_AllowedArea), "HeaderClicked")]
        private static void DoHeader(PawnTable table)
        {
            if (Event.current.alt && Find.CurrentMap != null)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                //无限制
                list.Add(new FloatMenuOption("NoAreaAllowed".Translate(), () =>
                {
                    table.PawnsListForReading.ForEach(p =>
                    {
                        if (p.Faction == Faction.OfPlayer && p.playerSettings.SupportsAllowedAreas)
                        {
                            p.playerSettings.AreaRestrictionInPawnCurrentMap = null;
                        }
                    });
                }));
                foreach (var area in Find.CurrentMap.areaManager.AllAreas.Where(a => a.AssignableAsAllowed()))
                {
                    list.Add(new FloatMenuOption(area.Label, () =>
                    {
                        table.PawnsListForReading.ForEach(p =>
                        {
                            if (p.Faction == Faction.OfPlayer && p.playerSettings.SupportsAllowedAreas)
                            {
                                p.playerSettings.AreaRestrictionInPawnCurrentMap = area;
                            }
                        });
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }

        //在制作衣物列表显示衣物的覆盖层
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Def), nameof(Def.LabelCap), MethodType.Getter)]
        private static void LabelCap(ref TaggedString __result, Def __instance)
        {
            if (__instance is RecipeDef recipe && recipe.ProducedThingDef.IsApparel)
            {
                __result += $" [{recipe.ProducedThingDef.apparel.GetLayersString()}]"
                    .Colorize(Color.cyan);
            }
        }
    }
}