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
    [HarmonyPatch(typeof(GeneUtility))]
    class MetabolismAfterImplanting
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GeneUtility.MetabolismAfterImplanting))]
        public static void MetabolismDefectDiscount
        (
            ref int __result,
            Pawn pawn
        )
        {
            if (pawn.CanMutate())
                __result -= pawn.Mutations().MetFromDefects;
        }
    }
}
