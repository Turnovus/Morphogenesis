using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Morphogenesis
{
    [HarmonyPatch(typeof(Pawn_GeneTracker))]
    class Notify_GenesChanged
    {
        [HarmonyPatch("Notify_GenesChanged")]
        public static void Postfix(
            Pawn_GeneTracker __instance,
            GeneDef addedOrRemovedGene
        )
        {
            if (__instance.pawn != null && __instance.pawn.CanMutate())
                __instance.pawn?.Mutations()?.Notify_GenesChanged(
                    addedOrRemovedGene);
        }
    }
}
