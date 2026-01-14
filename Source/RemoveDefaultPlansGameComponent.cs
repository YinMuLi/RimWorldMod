using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimEase.Source
{
    // 在这里可以通过开关选择要删除的数据库
    public class RemoveDefaultPlansGameComponent : GameComponent
    {
        public RemoveDefaultPlansGameComponent(Game game) : base()
        {
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            TryRemoveDefaults();
        }

        private void TryRemoveDefaults()
        {
            try
            {
                var outfitDb = Current.Game?.outfitDatabase;
                if (outfitDb != null)
                {
                    var outfits = outfitDb.AllOutfits.ToList();
                    for (int i = outfits.Count - 1; i >= 0; i--)
                    {
                        var o = outfits[i];
                        // 彻底移除
                        outfitDb.AllOutfits.Remove(o);
                    }
                    Log.Message("[RemoveDefaultPlans] 已删除所有 Outfit。");
                }

                var foodDb = Current.Game?.foodRestrictionDatabase;
                if (foodDb != null)
                {
                    var frs = foodDb.AllFoodRestrictions.ToList();
                    for (int i = frs.Count - 1; i >= 0; i--)
                    {
                        foodDb.AllFoodRestrictions.Remove(frs[i]);
                    }
                    Log.Message("[RemoveDefaultPlans] 已删除所有 FoodRestriction。");
                }

                var drugDb = Current.Game?.drugPolicyDatabase;
                if (drugDb != null)
                {
                    var dps = drugDb.AllPolicies.ToList();
                    for (int i = dps.Count - 1; i >= 0; i--)
                    {
                        drugDb.AllPolicies.Remove(dps[i]);
                    }
                    Log.Message("[RemoveDefaultPlans] 已删除所有 DrugPolicy。");
                }

                Log.Message("[RemoveDefaultPlans] 新开局默认方案清理完成。");

                var readDb = Current.Game?.readingPolicyDatabase;
                if (readDb != null)
                {
                    var rds = readDb.AllReadingPolicies.ToList();
                    for (int i = rds.Count - 1; i >= 0; i--)
                    {
                        readDb.AllReadingPolicies.Remove(rds[i]);
                    }
                    Log.Message("[RemoveDefaultPlans] 已删除所有ReadingPolicy。");
                }
            }
            catch (Exception ex)
            {
                Log.Error("[RemoveDefaultPlans] 清理默认方案时出现异常: " + ex);
            }
        }
    }
}