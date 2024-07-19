using HugsLib;
using RimWorld;
using System.Linq;
using UnityEngine;
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
            //配方
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                //在制作衣物列表显示衣物的覆盖层
                if (recipe.ProducedThingDef?.IsApparel ?? false)
                {
                    recipe.label += $" [{recipe.ProducedThingDef.apparel.GetLayersString()}]"
                        .Colorize(Color.cyan);
                }
            }
        }

        public override void Tick(int currentTick)
        {
            //TODO: 增加检测间隔
            if (Handles.autoResearch && Find.ResearchManager.GetProject() == null)
            {
                //当前研究项目为空，查找随机可以研究的项目
                var project = (DefDatabase<ResearchProjectDef>.AllDefs
                    .Where(p => p.CanStartNow)).RandomElement();
                Find.ResearchManager.SetCurrentProject(project);
            }
        }
    }
}