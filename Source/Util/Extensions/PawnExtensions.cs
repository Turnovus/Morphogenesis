using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    public static class PawnExtensions
    {
        public static CompMutationTracker Mutations(this Pawn pawn)
            => pawn.TryGetComp<CompMutationTracker>();

        public static bool CanMutate(this Pawn pawn)
        {
            CompMutationTracker mut = pawn.Mutations();
            return mut != null && mut.Props.allowMutations;
        }

        public static bool HasMutation(this Pawn pawn, Gene gene)
        {
            CompMutationTracker comp = pawn.Mutations();
            //Log.Message((comp != null && comp.HasMutation(gene)).ToString());
            return comp != null && comp.HasMutation(gene);
        }
    }
}
