using HugsLib;
using Verse;

namespace YinMu.Source
{
    internal class ModEntry : ModBase
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
                //改变3*1工作台的操作位置，统一在最右边，但是改了话有好有坏
                //if (thingDef.hasInteractionCell && thingDef.Size.x == 3)
                //{
                //    //第一位表示操作横向位置，负数向左，正数向右
                //    //第二位？？
                //    //第三位表示操作纵向位置，负数向下，正数向上
                //    thingDef.interactionCellOffset = new IntVec3(1, 0, -1);
                //}
            }
        }
    }
}