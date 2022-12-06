using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    public static class GeneExtensions
    {
        public static bool IsDefect(this Gene gene)
        {
            if (!gene.pawn.CanMutate()) return false;
            return gene.pawn.Mutations().HasDefect(gene);
        }
    }
}
