using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    public static class GeneDefExtensions
    {
        public static MutationProps Mutations(this GeneDef gene)
            => gene.GetModExtension<MutationProps>();
    }
}
