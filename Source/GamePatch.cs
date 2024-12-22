﻿using BetterGameLife.Source.Utils;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Verse.DamageWorker;

namespace BetterGameLife.Source
{
    /// <summary>
    /// 类上一定要加[Harmony]
    /// </summary>
    [Harmony]
    internal class GamePatch
    {
        /// <summary>
        /// 建筑返还材料
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.TryDrop))]
        [HarmonyPatch(new Type[] { typeof(Apparel), typeof(Apparel), typeof(IntVec3), typeof(bool) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
        private static bool TryDrop(Pawn_ApparelTracker __instance, Apparel ap, ref bool __result)
        {
            if (ModEntry.Instance.Handles.OnlyDropSmeltableApperal && __instance.pawn.Dead && !ap.Smeltable)
            {
                __result = false;
                return false;
            }
            return true;
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
                //if (Mouse.IsOver(rect1))
                //{
                //    Widgets.DrawHighlight(rect1);
                //}
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
            if (__instance == null || __result == null || dinfo.Instigator == null)
            {
                return;
            }
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

        #region 机械师无视距离控制机器（不过我也不怎么玩机械师）

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

        #endregion 机械师无视距离控制机器（不过我也不怎么玩机械师）

        //不显示泰南语
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Translator), "PseudoTranslated")]
        private static bool PseudoTranslated(string original, ref string __result)
        {
            __result = original;
            return false;
        }

        #region 简化游戏初始界面

        //隐藏左侧翻译框（很感激翻译人员的！！）
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoTranslationInfoRect))]
        private static bool DoTranslationInfoRect() => false;

        //隐藏左下方DLC的信息
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoExpansionIcons))]
        private static bool DoExpansionIcons() => false;

        #endregion 简化游戏初始界面

        //在允许活动区标签上ALT键+左/右键点击，快捷更改活动区域
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PawnColumnWorker_AllowedArea), "HeaderClicked")]
        private static bool DoHeader(PawnTable table)
        {
            //base.HeaderClicked：排序鼠标左键，右键也是排序
            if ((Event.current.button == 0 || Event.current.button == 1) && Find.CurrentMap != null)
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
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            }
            return false;
        }

        #region 删减开局管理中的一些没用配置

        //食物
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(FoodRestrictionDatabase), "GenerateStartingFoodRestrictions")]
        //private static bool GenerateStartingFoodRestrictions(FoodRestrictionDatabase __instance)
        //{
        //    __instance.MakeNewFoodRestriction().label = "FoodRestrictionLavish".Translate();
        //    return false;
        //}

        ////穿着
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(OutfitDatabase), "GenerateStartingOutfits")]
        //private static bool GenerateStartingOutfits(OutfitDatabase __instance)
        //{
        //    __instance.MakeNewOutfit().label = "OutfitAnything".Translate();
        //    return false;
        //}

        //成瘾品的配置是写在XML中的

        #endregion 删减开局管理中的一些没用配置

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(GlowGrid), "GroundGlowAt")]
        //private static void GroundGlowAt(ref float __result, IntVec3 c, Map ___map)
        //{
        //    if (___map.roofGrid.RoofAt(c) == RoofDefOf.RoofConstructed)
        //    {
        //        float num = ___map.skyManager.CurSkyGlow;
        //        if (num > __result)
        //        {
        //            __result = num;
        //        }
        //    }
        //}
        //彩色品质
        public static void ColorQuality(QualityCategory cat, ref string __result)
        {
            switch (cat)
            {
                case QualityCategory.Legendary: __result = __result.Colorize(new Color(255, 215, 0)); break;//金色
                case QualityCategory.Masterwork: __result = __result.Colorize(Color.red); break;
                case QualityCategory.Excellent: __result = __result.Colorize(new Color(128, 0, 128)); break;//紫色
                case QualityCategory.Good: __result = __result.Colorize(Color.green); break;
                //case QualityCategory.Normal: __result = __result.Colorize(Color.black); break;
                case QualityCategory.Poor: __result = __result.Colorize(Color.gray); break;
                case QualityCategory.Awful: __result = __result.Colorize(Color.gray); break;
            }
        }

        //立刻关闭电源，还有这种写法！！！
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CompFlickable), "<CompGetGizmosExtra>b__20_1")]
        private static bool CompGetGizmosExtra(ref bool ___wantSwitchOn, CompFlickable __instance)
        {
            if (ModEntry.Instance.Handles.TogglePowerInstantly)
            {
                ___wantSwitchOn = !___wantSwitchOn;
                __instance.DoFlick();

                return false;
            }
            return true;
        }

        //立刻开门，谁便写写竟然成功了
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Building_Door), "<GetGizmos>b__69_1")]
        private static bool Building_Door_GetGizmos(ref bool ___holdOpenInt, Building_Door __instance)
        {
            if (ModEntry.Instance.Handles.ToggleDoorOpenedInstantly)
            {
                ___holdOpenInt = !___holdOpenInt;
                if (___holdOpenInt)//保持敞开
                {
                    AccessTools.Method(typeof(Building_Door), "DoorOpen")?.Invoke(__instance, new object[] { 110 });
                }
                else
                {
                    AccessTools.Method(typeof(Building_Door), "DoorTryClose")?.Invoke(__instance, null);
                }
                return false;
            }

            return true;
        }

        #region 智能控制遇到时间的速度

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TimeControls), nameof(TimeControls.DoTimeControlsGUI))]
        private static void DoTimeControlsGUI(Rect timerRect)
        {
            if (Mouse.IsOver(timerRect) && Find.TickManager.slower.ForcedNormalSpeed
                && (Event.current.button == 1)
                && Event.current.type == EventType.MouseDown)
            //限速情况下，鼠标右键
            {
                var list = new List<FloatMenuOption>
                {
                    new FloatMenuOption("NormalSpeed".Translate(),()=>{ModSettings.curSpeed=SpeedOption.Normal;}),
                    new FloatMenuOption("FastSpeed".Translate(),()=>{ModSettings.curSpeed=SpeedOption.Fast;}),
                    new FloatMenuOption("SuperfastSpeed".Translate(),()=>{ModSettings.curSpeed=SpeedOption.Superfast;}),
                    new FloatMenuOption("UltrafastSpeed".Translate(),()=>{ModSettings.curSpeed=SpeedOption.Ultrafast;}),
                };
                var window = new FloatMenu(list);
                Find.WindowStack.Add(window);
                Event.current.Use();
            }
        }

        private static readonly Func<TickManager, bool> NothingHappeningInGame = AccessTools.MethodDelegate<Func<TickManager, bool>>("TickManager:NothingHappeningInGame");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TickManager), nameof(TickManager.TickRateMultiplier), MethodType.Getter)]
        private static void TickRateMultiplier(ref float __result, bool ___UltraSpeedBoost, TickManager __instance)
        {
            if (Find.TickManager.slower.ForcedNormalSpeed)
            {
                if (__instance.CurTimeSpeed == TimeSpeed.Paused)
                {
                    __result = 0f;
                }
                switch (ModSettings.curSpeed)
                {
                    case SpeedOption.Fast: __result = 3f; break;
                    case SpeedOption.Superfast:
                        if (Find.Maps.Count == 0)
                        {
                            __result = 120f;
                        }
                        if (NothingHappeningInGame.Invoke(__instance))
                        {
                            __result = 12f;
                        }
                        __result = 6f;
                        break;

                    case SpeedOption.Ultrafast:
                        if (Find.Maps.Count == 0 || ___UltraSpeedBoost)
                        {
                            __result = 150f;
                        }
                        __result = 15f;
                        break;
                }
            }
            else { ModSettings.curSpeed = SpeedOption.Normal; }
        }

        #endregion 智能控制遇到时间的速度

        //private static readonly Action<TransferableOneWayWidget> CacheTransferables = AccessTools.MethodDelegate<Action<TransferableOneWayWidget>>("TransferableOneWayWidget:CacheTransferables");
        //private static Type InnerSection = AccessToolsExtensions.InnerTypes(typeof(TransferableOneWayWidget)).FirstOrDefault();

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(TransferableOneWayWidget), nameof(TransferableOneWayWidget.OnGUI))]
        //[HarmonyPatch([typeof(Rect), typeof(bool)], [ArgumentType.Normal, ArgumentType.Out])]
        //private static void OnGUI(TransferableOneWayWidget __instance)
        //{
        //    GUI.BeginGroup(new Rect(540f, 0f, 525f, 27f));
        //    //Text.Font = GameFont.Tiny;
        //    //Text.Anchor = TextAnchor.LowerCenter;
        //    if (Widgets.ButtonText(new Rect(0f, 0f, 80f, 27f), ModEntry.Instance.itemCategory.Label))
        //    {
        //        List<FloatMenuOption> list = new List<FloatMenuOption>();
        //        list.Add(new FloatMenuOption("AllCategory".Translate(), () =>
        //        {
        //            ModEntry.Instance.itemCategory = new ItemCategory();
        //            CacheTransferables.Invoke(__instance);
        //        }));
        //        foreach (var item in DefDatabase<ThingCategoryDef>.AllDefsListForReading
        //            .Where(t => t.parent == ThingCategoryDefOf.Root))
        //        {
        //            list.Add(new FloatMenuOption(item.LabelCap, () =>
        //            {
        //                ModEntry.Instance.itemCategory = new ItemCategory(item);
        //                CacheTransferables.Invoke(__instance);
        //            }));
        //        }

        //        Find.WindowStack.Add(new FloatMenu(list));
        //    }
        //    Widgets.EndGroup();
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(TransferableOneWayWidget), "CacheTransferables")]
        //private static void CacheTransferables_Post()
        //{
        //    //section是个内部结构类，不会获取
        //}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.TipString))]
        private static void TraitTipString(ref string __result, Trait __instance)
        {
            __result += $"\n<b><color=#45B39D><{__instance.def.modContentPack.Name}></color></b>";
        }
    }
}