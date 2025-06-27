using RimWorld;
using Verse;

namespace RimEase.Source.Utils
{
    internal static class Extensions
    {
        public static bool IsFactionOfPlayer(this Pawn pawn)
        {
            return pawn.Faction != null && pawn.Faction == Faction.OfPlayer;
        }
    }
}