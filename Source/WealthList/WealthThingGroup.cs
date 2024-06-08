using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YinMu.Source.WealthList
{
    public class WealthThingGroup
    {
        public string name;
        public List<WealthThing> things = [];
        public bool expanded;//是否已经展开
        public float Wealth => things.Sum(t => t.wealth);

        public WealthThingGroup(string name, List<Thing> list, Func<Thing, float> func)
        {
            this.name = name;
            Add(list, func);
        }

        public void Add(List<Thing> list, Func<Thing, float> singleValue)
        {
            var res = list.GroupBy(t => $"{t.def}{singleValue(t)}")
                .Select(group => new WealthThing
                {
                    thing = group.RandomElement(),
                    wealth = group.Sum(t => singleValue(t) * t.stackCount),
                    count = group.Sum(t => t.stackCount)
                });
            things.AddRange(res);
        }

        public void Desc()
        {
            //第一是按财富排名，第二是按名字长度
            things.SortByDescending(t => t.wealth, t => t.DisplayName);
        }
    }

    public struct WealthThing
    {
        public Thing thing;
        public float wealth;
        public int count;

        public readonly string DisplayName
        {
            get
            {
                //Label=>LabelNoCount + " x" + stackCount.ToStringCached()
                //LabelNoCount => GenLabel.ThingLabel(this, 1)
                if (count > 1) return $"{thing.LabelNoParenthesis} x{count}";
                return thing.LabelNoCount;
            }
        }
    }
}