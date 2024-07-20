using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace YinMu.Source.ArrestHere
{
    internal class CompProperties_Imprisonment : CompProperties
    {
        public CompProperties_Imprisonment()
        {
            this.compClass = typeof(Comp_Imprisonment);
        }
    }

    internal class Comp_Imprisonment : ThingComp
    {
        public int lastTryingEscapeTick;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastTryingEscapeTick, "lastTryingEscapeTick");
        }
    }
}