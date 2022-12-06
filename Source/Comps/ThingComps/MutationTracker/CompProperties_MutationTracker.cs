using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    public class CompProperties_MutationTracker : CompProperties
    {
        public bool allowMutations = true;

        public CompProperties_MutationTracker() =>
            compClass = typeof(CompMutationTracker);
    }
}
