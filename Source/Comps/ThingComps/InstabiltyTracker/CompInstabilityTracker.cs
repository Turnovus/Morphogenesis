using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    class CompInstabilityTracker : ThingComp
    {
        public double baseInstability = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref baseInstability, "baseInstability");
        }
    }
}
