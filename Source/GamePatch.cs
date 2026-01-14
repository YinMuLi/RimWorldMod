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

namespace RimEase.Source
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
            if (__result != null && treeDestructionMode == PlantDestructionMode.Chop)
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

        #region 机械师无视距离控制机器（不过我也不怎么玩机械师，收回前话机械族真香）

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

        #endregion 机械师无视距离控制机器（不过我也不怎么玩机械师，收回前话机械族真香）

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

        //彩色品质

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GetLabel), new Type[] { typeof(QualityCategory) })]
        private static void ColorQuality(QualityCategory cat, ref string __result)
        {
            if (string.IsNullOrEmpty(__result)) return;

            // 十六进制颜色映射（基于你给出的 RGB 值）
            string hex = cat switch
            {
                QualityCategory.Legendary => "E6AF19", // 230,175,25 深金色
                QualityCategory.Masterwork => "DC3C3C", // 220,60,60 暗红
                QualityCategory.Excellent => "A046C8", // 160,70,200 亮紫
                QualityCategory.Good => "41AA46", // 65,170,70 军绿色
                QualityCategory.Normal => "646464", // 100,100,100 中灰
                QualityCategory.Poor => "8C8C8C", // 140,140,140 浅灰
                QualityCategory.Awful => "464646", // 70,70,70 深灰
                _ => "FFFFFF"
            };

            // 使用 Unity Rich Text 手动包裹 hex（确保格式为 #RRGGBB）
            __result = $"<color=#{hex}>{__result}</color>";
        }
    }
}