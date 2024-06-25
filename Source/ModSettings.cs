using HugsLib;
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
               "ModSettings.AutoChopStumps".Translate(),
               "ModSettings.AutoChopStumpsDesc".Translate(), true
               );
            betterSpoils = pack.GetHandle(
               "betterSpoils",
               "ModSettings.BetterSpoils".Translate(),
               "ModSettings.BetterSpoilsDesc".Translate(), true
               );
        }
    }
}