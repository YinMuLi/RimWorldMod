using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YinMu.Source.ShowTrade
{
    internal class TradeableGroup
    {
        public bool expanded = true;
        public List<Tradeable> list = [];
        public string name;

        public TradeableGroup()
        { }

        public TradeableGroup(ThingCategoryDef def, List<Tradeable> tradeables)
        {
            list.AddRange(tradeables.Where(tr => tr.ThingDef.IsWithinCategory(def)));
            name = def.LabelCap;
        }
    }
}