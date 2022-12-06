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
    [HarmonyPatch(typeof(StatPart_MetabolismTotal))]
    class CurveXGetter
    {
        [HarmonyPostfix]
        [HarmonyPatch("CurveXGetter")]
        public static void NegateDefectMetabolism
        (
            ref float __result,
            StatRequest req
        )
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn.CanMutate())
                __result -= pawn.Mutations().MetFromDefects;
        }
    }
}
