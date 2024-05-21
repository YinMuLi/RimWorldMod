using HugsLib;
using RimWorld;
using System.Collections.Generic;
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
                if (!GenList.NullOrEmpty<ThingCategoryDef>((IList<ThingCategoryDef>)thingDef.thingCategories))
                {
                    if ((thingDef.thingCategories[0].parent == ThingCategoryDefOf.ResourcesRaw))
                    {
                        Logger.Message($"大爱仙尊：{thingDef.label}->{thingDef.stackLimit}");
                    }
                }
            }
        }
    }
}