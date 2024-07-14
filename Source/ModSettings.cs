using HugsLib.Settings;
using Verse;

namespace YinMu.Source
{
    /// <summary>
    /// 模组的设置暂时这么写
    /// </summary>
    internal class ModSettings
    {
        public SettingHandle<bool> autoChopStumps;//自动砍伐树桩

        /// <summary>
        /// spoils:战利品
        /// </summary>
        public SettingHandle<bool> betterSpoils;

        public void Read(ModSettingsPack pack)
        {
            autoChopStumps = pack.GetHandle(
               "autoChopStumps",
               "Settings.AutoChopStumps".Translate(),
               "Settings.AutoChopStumpsDesc".Translate(), true
               );
            betterSpoils = pack.GetHandle(
               "betterSpoils",
               "Settings.BetterSpoils".Translate(),
               "Settings.BetterSpoilsDesc".Translate(), true
               );
        }
    }
}