using HarmonyLib;
using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Verse;
using static UnityEngine.GridBrushBase;

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
            }
        }
    }
}