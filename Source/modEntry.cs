using HarmonyLib;
using HugsLib;
using HugsLib.Utils;
using RimWorld;
using System.Collections;
using System.Linq;
using Verse;

namespace RimEase.Source
{
    [EarlyInit]
    internal class ModEntry : ModBase
    {
        public ModLogger Debug = new ModLogger("RimEase");

        public ModEntry()
        {
            Instance = this;
            //itemCategory = new ItemCategory();
            HarmonyInst = new Harmony("YinMu.RimEase");
            HarmonyInst.Patch(AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GetLabel)), postfix: new HarmonyMethod(typeof(GamePatch), nameof(GamePatch.ColorQuality)));
            HarmonyInst.Patch(AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GetLabelShort)), postfix: new HarmonyMethod(typeof(GamePatch), nameof(GamePatch.ColorQuality)));
            HarmonyInst.PatchAll();
        }

        public static ModEntry Instance { get; private set; }

        /// <summary>
        /// Handles:处理
        /// </summary>
        public ModSettings Handles { get; private set; }

        public override string ModIdentifier => "RimEase";
        protected override bool HarmonyAutoPatch => false;

        public override void DefsLoaded()
        {
            if (!ModIsActive) return;
            Handles.Read(Settings);
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                //所有堆叠数大于一的物品存储容量*20（SOS2不太够用）
                if (thingDef.stackLimit > 1) thingDef.stackLimit *= 20;

                //需要研究的物品，大多是Boss掉落，提高耐久度
                if (thingDef.HasComp<CompAnalyzableUnlockResearch>())
                {
                    //信息显示界面上还会显示耐久度
                    thingDef.SetStatBaseValue(StatDefOf.Flammability, 0f);//易燃
                    thingDef.SetStatBaseValue(StatDefOf.DeteriorationRate, 0f);//老化速度
                    thingDef.useHitPoints = false;
                }
            }

            foreach (var type in GenDefDatabase.AllDefTypesWithDatabases())
            {
                foreach (var def in (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), type, "AllDefs"))
                {
                    AddModInfo((Def)def);
                }
            }
        }

        public override void EarlyInitialize()
        {
            Handles = new ModSettings();
        }

        public override void Tick(int currentTick)
        {
            //currentTick会一直增加
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

        private void AddModInfo(Def def)
        {
            if (def.modContentPack != null)
            {
                def.description += $"\n<i><b><color=#45B39D>{def.modContentPack.Name}</color></b></i>";
            }
        }
    }
}