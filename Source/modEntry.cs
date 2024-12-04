using HarmonyLib;
using HugsLib;
using HugsLib.Utils;
using RimWorld;
using System.Collections;
using System.Linq;
using UnityEngine;
using Verse;

namespace BetterGameLife.Source
{
    [EarlyInit]
    internal class ModEntry : ModBase
    {
        public override string ModIdentifier => "BetterGameLife";

        /// <summary>
        /// Handles:处理
        /// </summary>
        public ModSettings Handles { get; private set; }

        public static ModEntry Instance { get; private set; }
        public ModLogger ModLogger = new ModLogger("BetterGameLife");

        private ModEntry()
        {
            Instance = this;
            var harmony = new Harmony("YinMu");
            harmony.Patch(AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GetLabel)), postfix: new HarmonyMethod(typeof(GamePatch), nameof(GamePatch.ColorQuality)));
            harmony.Patch(AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GetLabelShort)), postfix: new HarmonyMethod(typeof(GamePatch), nameof(GamePatch.ColorQuality)));
        }

        public override void EarlyInitialize()
        {
            Handles = new ModSettings();
        }

        public override void StaticInitialize()
        {
            //foreach (var pawn in DefDatabase<ThingDef>
            //    .AllDefs.Where(t => t.race?.Humanlike ?? false))
            //{
            //    pawn.comps.Add(new CompProperties_Imprisonment());
            //}
        }

        public override void DefsLoaded()
        {
            if (!ModIsActive) return;
            Handles.Read(Settings);
            var rot = DefDatabase<ThingCategoryDef>.GetNamed("OrganicMatter");
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                //所有堆叠数大于一的物品存储容量*20（SOS2不太够用）
                if (thingDef.stackLimit > 1) thingDef.stackLimit *= 20;
                //不是尸体，有腐烂度
                //TODO:动态创建新的分组，就用thingDef的分组名称
                if (thingDef.HasComp<CompRottable>() && !thingDef.IsCorpse)
                {
                    thingDef.thingCategories.Add(rot);
                    rot.childThingDefs.Add(thingDef);
                }
            }
            rot.ClearCachedData();
            rot.ResolveReferences();

            //配方
            //foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
            //{
            //    //在制作衣物列表显示衣物的覆盖层
            //    if (recipe.ProducedThingDef?.IsApparel ?? false)
            //    {
            //        recipe.label += $" [{recipe.ProducedThingDef.apparel.GetLayersString()}]"
            //            .Colorize(Color.cyan);
            //    }
            //}
            foreach (var type in GenDefDatabase.AllDefTypesWithDatabases())
            {
                foreach (var def in (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), type, "AllDefs"))
                {
                    AddModInfo((Def)def);
                }
            }
        }

        private void AddModInfo(Def def)
        {
            if (def.modContentPack != null)
            {
                //def.description += $"\n<b><color=#545454>{def.modContentPack.Name}</color></b>";
                def.description += $"\n<b>{def.modContentPack.Name.Colorize(Color.gray)}</b>";
            }
        }

        public override void Tick(int currentTick)
        {
            //TODO: 增加检测间隔
            if (Handles.autoResearch && Find.ResearchManager.GetProject() == null)
            {
                //查找可研究项目
                var project = (DefDatabase<ResearchProjectDef>.AllDefs
                    .Where(p => p.CanStartNow)).RandomElement();
                //检测研究台与研究设备，如果是基础研究台requiredResearchBuilding=null
                //删繁就简
                if (project != null) Find.ResearchManager.SetCurrentProject(project);
            }
        }
    }
}