using RimWorld;
using Verse;

namespace BetterGameLife.Source
{
    internal static class Extensions
    {
        public static bool IsFactionOfPlayer(this Pawn pawn)
        {
            return pawn.Faction != null && pawn.Faction == Faction.OfPlayer;
        }
    }
}