using HugsLib;
using RimWorld;
using Verse;

namespace YinMu.Source
{
    internal class modEntry : ModBase
    {
        public override void DefsLoaded()
        {
            if (!ModIsActive) return;

            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                //所有堆叠数大于一的物品存储容量*10
                if (thingDef.stackLimit > 1) thingDef.stackLimit *= 10;
                //水培种植所有 条件：能种在地上，不是树木
                if (thingDef.plant != null && !thingDef.plant.IsTree &&
                    !thingDef.plant.sowTags.Contains("Hydroponic") &&
                    thingDef.plant.sowTags.Contains("Ground"))
                {
                    thingDef.plant.sowTags.Add("Hydroponic");
                }
            }
        }
    }
}