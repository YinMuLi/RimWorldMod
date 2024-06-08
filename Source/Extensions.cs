using RimWorld;
using System.Linq;
using Verse;

namespace YinMu.Source
{
    internal static class Extensions
    {
        /// <summary>
        /// 判断销售的商品是否属于某一类别
        /// </summary>
        /// <param name="tradeable"></param>
        /// <param name="def">商品类别</param>
        /// <returns></returns>
        public static bool Is(this Tradeable tr, ThingCategoryDef def)
        {
            return tr.ThingDef.thingCategories != null && tr.ThingDef.thingCategories.Contains(def)
                || GenCollection.Any<ThingCategoryDef>(tr.ThingDef.thingCategories.FindAll(c => c.Parents.Contains(def)));
        }
    }
}