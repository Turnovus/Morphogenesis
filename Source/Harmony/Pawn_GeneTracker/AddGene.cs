using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Morphogenesis
{
    [HarmonyPatch(typeof(Pawn_GeneTracker))]
    class AddGene
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pawn_GeneTracker.AddGene))]
        [HarmonyPatch(new Type[] { typeof(GeneDef), typeof(bool) })]
        public static bool AddUnmarksMutations
        (
            Pawn_GeneTracker __instance,
            GeneDef geneDef,
            bool xenogene
        )
        {
            Pawn pawn = __instance.pawn;
            if (!xenogene || pawn == null || !pawn.CanMutate() ||
                !pawn.Mutations().HasMutation(geneDef))
                return true;

            pawn.Mutations().UnmarkAsMutation(geneDef);
            return false;
        }
    }
}
