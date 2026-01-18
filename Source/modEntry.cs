using HarmonyLib;
using RimWorld;
using System.Collections;
using Unity.Burst.Intrinsics;
using Verse;

namespace RimEase.Source
{
    internal class ModEntry : Mod
    {
        public ModEntry(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("YinMu.RimEase");
            harmony.Patch(AccessTools.Method(typeof(QualityUtility), "GetLabel"),
                       prefix: null,
                       postfix: new HarmonyMethod(typeof(GamePatch), nameof(GamePatch.ColorQuality)));
            harmony.Patch(AccessTools.Method(typeof(QualityUtility), "GetLabelShort"),
                       prefix: null,
                       postfix: new HarmonyMethod(typeof(GamePatch), nameof(GamePatch.ColorQuality)));
            harmony.PatchAll();
            Log.Message("RimEase模组：Harmony补丁已注入");
            LongEventHandler.QueueLongEvent(ForEachDef, "WTM_LoadingMsg", false, null);
        }

        private void ForEachDef()
        {
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

        private void AddModInfo(Def def)
        {
            if (!def.description.NullOrEmpty() && def.modContentPack != null && !def.description.Contains(def.modContentPack.Name))
            {
                def.description += $"\n\n<color=#45B39D>来自模组: {def.modContentPack.Name}</color>";
            }
        }
    }
}