using HugsLib.Settings;
using Verse;

namespace BetterGameLife.Source
{
    /// <summary>
    /// 模组的设置暂时这么写
    /// </summary>
    internal class ModSettings
    {
        public SettingHandle<bool> autoChopStumps;//自动砍伐树桩
        public SettingHandle<bool> autoResearch;//自动研究

        /// <summary>
        /// spoils:战利品
        /// </summary>
        public SettingHandle<bool> betterSpoils;

        public void Read(ModSettingsPack pack)
        {
            autoChopStumps = pack.GetHandle(
               "AutoChopStumps",
               "AutoChopStumps".Translate(),
               "AutoChopStumpsDesc".Translate(), true
               );
            betterSpoils = pack.GetHandle(
               "BetterSpoils",
               "BetterSpoils".Translate(),
               "BetterSpoilsDesc".Translate(), true
               );
            autoResearch = pack.GetHandle(
             "AutoResearch",
             "AutoResearch".Translate(),
             "AutoResearchDesc".Translate(), true
             );
        }
    }
}