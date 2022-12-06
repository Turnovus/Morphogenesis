using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    [StaticConstructorOnStartup]
    public static class MutationUtils
    {
#pragma warning disable CS0649
        private static Dictionary<int, List<GeneDef>> availableMutations;
        private static Dictionary<int, float> metWeights;
        private static Dictionary<GeneDef, GeneDef_Display> displayGeneDefs;
        private static Dictionary<GeneDef, GeneDef_Display> defectGeneDefs;
        private static Dictionary<GeneDef, GeneDef_Display> dormantGeneDefs;
#pragma warning restore CS0649

        static MutationUtils()
        {
            availableMutations = new Dictionary<int, List<GeneDef>>();
            metWeights = new Dictionary<int, float>();
            displayGeneDefs = new Dictionary<GeneDef, GeneDef_Display>();
            defectGeneDefs = new Dictionary<GeneDef, GeneDef_Display>();
            dormantGeneDefs = new Dictionary<GeneDef, GeneDef_Display>();

            foreach (
                GeneDef gene in DefDatabase<GeneDef>.AllDefsListForReading
            )
            {
                if (CanBeMutation(gene))
                {
                    // Set up all possible mutations, sorted by metabolism
                    if (!availableMutations.ContainsKey(gene.biostatMet))
                        availableMutations[gene.biostatMet] =
                            new List<GeneDef>();
                    availableMutations[gene.biostatMet].Add(gene);

                    // Copy and save defs for inner genes
                    if (CanBeMutagene(gene))
                    {
                        GeneDef_Display defI = new GeneDef_Display(
                            gene, GeneDef_Display.EDisplayType.Mutagene);
                        displayGeneDefs[gene] = defI;
                    }
                    if (CanBeDefect(gene))
                    {
                        GeneDef_Display defect = new GeneDef_Display(
                            gene, GeneDef_Display.EDisplayType.Defect);
                        defectGeneDefs[gene] = defect;

                        GeneDef_Display dormant = new GeneDef_Display(
                            gene, GeneDef_Display.EDisplayType.DefectHidden);
                        dormantGeneDefs[gene] = dormant;
                    }
                }
            }

            // Set up the average selection weights of each metabolism group
            foreach (int i in availableMutations.Keys)
                metWeights[i] = (float)availableMutations[i].Average(x =>
                {
                    MutationProps props = x.Mutations();
                    return props == null ? 1f : props.baseMutageneWeight;
                });
        }

        public static bool CanBeMutation(GeneDef def)
        {
            if (def.biostatArc > 0) return false;
            MutationProps props = def.Mutations();
            return props == null || props.canBeDefect || props.canBeMutagene;
        }

        public static bool CanBeMutagene(GeneDef def)
        {
            if (def.biostatArc > 0) return false;
            MutationProps props = def.Mutations();
            return props == null || props.canBeMutagene;
        }

        public static bool CanBeDefect(GeneDef def)
        {
            if (def.biostatArc > 0) return false;
            MutationProps props = def.Mutations();
            return props != null && props.canBeDefect;
        }

        public static GeneDef GetAnyDefectFor(Pawn pawn)
        {
            if (pawn == null || !pawn.CanMutate()) return null;
            return GetAnyDefectFor(pawn.Mutations());
        }

        public static GeneDef GetAnyDefectFor(CompMutationTracker comp)
        {
            if (comp == null) return null;

            List<GeneDef> validGenes = defectGeneDefs.Keys.Where(g =>
            {
                return comp.CanAcceptDefect(g);
            }).ToList();
            if (validGenes.NullOrEmpty()) return null;

            return validGenes.RandomElementByWeight(g =>
            {
                MutationProps props = g.Mutations();
                return props == null ? 1f : props.baseDefectWeight;
            });
        }

        public static GeneDef_Display GetMutageneDefFor(GeneDef gene) =>
            displayGeneDefs.ContainsKey(gene) ? displayGeneDefs[gene] : null;

        public static GeneDef_Display GetDefectDefFor(GeneDef gene) =>
            defectGeneDefs.ContainsKey(gene) ? defectGeneDefs[gene] : null;

        public static GeneDef_Display GetDefectHiddenDefFor(GeneDef gene) =>
            dormantGeneDefs.ContainsKey(gene) ? dormantGeneDefs[gene] : null;
    }
}
