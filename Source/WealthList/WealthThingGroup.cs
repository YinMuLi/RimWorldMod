using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YinMu.Source.WealthList
{
    public class WealthThingGroup
    {
        public string name;
        public List<Wealth_Thing> things = [];
        public bool expanded;//是否已经展开
        public float Wealth => things.Sum(t => t.wealth);

        public WealthThingGroup(string name, List<Thing> list, Func<Thing, float> func)
        {
            this.name = name;
            Add(list, func);
        }

        public void Add(List<Thing> list, Func<Thing, float> func)
        {
            var res = list.GroupBy(t => t.LabelNoCount)
                .Select(group => new Wealth_Thing
                {
                    thing = group.RandomElement(),
                    wealth = group.Sum(t => func(t)),
                    count = group.Sum(t => t.stackCount)
                });
            things.AddRange(res);
        }

        public void Desc()
        {
            things.SortByDescending(t => t.wealth);
        }
    }

    public struct Wealth_Thing
    {
        public Thing thing;
        public float wealth;
        public int count;

        public readonly string DisplayName
        {
            get
            {
                if (count > 1) return $"{thing.LabelCapNoCount}X{count}";
                return thing.LabelNoCount;
            }
        }
    }
}