using HarmonyLib;
using Verse;

namespace YinMu.Source
{
    [StaticConstructorOnStartup]
    internal class modEntry
    {
        static modEntry()
        {
            var harmony = new Harmony("YinMu");
            harmony.PatchAll();
        }
    }
}