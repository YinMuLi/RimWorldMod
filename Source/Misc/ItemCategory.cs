//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;

//namespace BetterGameLife.Source.Misc
//{
//    /// <summary>
//    /// 物品分类，用于远征队与其它类似界面的物品分类
//    /// </summary>
//    internal struct ItemCategory
//    {
//        public ThingCategoryDef def;
//        public string Label => def == null ? "AllCategory".Translate() : def.LabelCap;

//        public ItemCategory(ThingCategoryDef def)
//        {
//            this.def = def;
//        }

//        public ItemCategory()
//        {
//            def = null;
//        }
//    }
//}