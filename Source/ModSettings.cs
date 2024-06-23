using HugsLib;
using HugsLib.Settings;
using Verse;

namespace YinMu.Source
{
    /// <summary>
    /// 模组的设置暂时这么写
    /// </summary>
    internal class ModSettings : ModBase
    {
        protected override bool HarmonyAutoPatch => false;
        public static ModSettings Instance { get; private set; }

        public override string ModIdentifier => "YinMu";

        //--------------------------------------------//
        public SettingHandle<bool> autoChopStumps;//自动砍伐树桩

        /// <summary>
        /// spoils:战利品
        /// </summary>
        public SettingHandle<bool> betterSpoils;

        private ModSettings()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            if (!ModIsActive) return;
            autoChopStumps = Settings.GetHandle<bool>(
                "autoChopStumps",
                "Settings.AutoChopStumps".Translate(),
                "Settings.AutoChopStumpsDesc".Translate(), true
                );
            betterSpoils = Settings.GetHandle<bool>(
               "betterSpoils",
               "Settings.BetterSpoils".Translate(),
               "Settings.BetterSpoilsDesc".Translate(), true
               );
        }
    }
}