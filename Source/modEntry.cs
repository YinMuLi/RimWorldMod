﻿using HugsLib;
using RimWorld;
using Verse;

namespace YinMu.Source
{
    [EarlyInit]
    internal class ModEntry : ModBase
    {
        public override string ModIdentifier => "YinMu";

        /// <summary>
        /// Handles:处理
        /// </summary>
        public ModSettings Handles { get; private set; }

        public static ModEntry Instance { get; private set; }

        private ModEntry()
        { Instance = this; }

        public override void EarlyInitialize()
        {
            Handles = new ModSettings();
        }

        public override void DefsLoaded()
        {
            if (!ModIsActive) return;
            Handles.Read(Settings);
            var rot = DefDatabase<ThingCategoryDef>.GetNamed("RottableThing");
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                //所有堆叠数大于一的物品存储容量*10（SOS2不太够用）
                if (thingDef.stackLimit > 1) thingDef.stackLimit *= 50;
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
        }
    }
}