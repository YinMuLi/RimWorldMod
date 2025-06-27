using RimEase.Source.Utils;
using HugsLib.Settings;
using Verse;

namespace RimEase.Source
{
    /// <summary>
    /// 模组的设置暂时这么写
    /// </summary>
    internal class ModSettings
    {
        public SettingHandle<bool> autoResearch;//自动研究
        public SettingHandle<bool> TogglePowerInstantly;//立刻切换电源
        public SettingHandle<bool> ToggleDoorOpenedInstantly;//立刻开门

        /// <summary>
        /// spoils:战利品
        /// </summary>
        public SettingHandle<bool> betterSpoils;

        public void Read(ModSettingsPack pack)
        {
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
            TogglePowerInstantly = pack.GetHandle(
            "TogglePowerInstantly",
            "TogglePowerInstantly".Translate(),
            "TogglePowerInstantlyDesc".Translate(), true
            );
            ToggleDoorOpenedInstantly = pack.GetHandle(
                "ToggleDoorOpenedInstantly",
                 "ToggleDoorOpenedInstantly".Translate(),
                 "ToggleDoorOpenedInstantlyDesc".Translate(),
                 true);
        }
    }
}