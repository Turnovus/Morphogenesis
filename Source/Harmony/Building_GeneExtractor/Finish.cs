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
    [HarmonyPatch(typeof(Building_GeneExtractor))]
    class Finish
    {
        static MethodInfo M_Initialize = typeof(Genepack).GetMethod(
            nameof(Genepack.Initialize));

        [HarmonyTranspiler]
        [HarmonyPatch("Finish")]
        public static IEnumerable<CodeInstruction> AddInstabilityToGenepack
        (
            IEnumerable<CodeInstruction> instructions
        )
        {
            Log.Message(M_Initialize == null ? "NULL" : M_Initialize.ToString());
            bool patched = false;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (patched ||
                    !(instruction.opcode == OpCodes.Callvirt &&
                    instruction.operand as MethodInfo == M_Initialize)
                )
                    continue;

                patched = true;
                // genepack
                yield return new CodeInstruction(OpCodes.Ldloc_2);
                // containedPawn
                yield return new CodeInstruction(OpCodes.Ldloc_1);

                yield return new CodeInstruction(
                    OpCodes.Call, AuxMethods.M_AdjustInstabilty);
            }
            if (!patched)
                Utils.Error("Transpiler AddInstabilityToGenepack failed!");
        }
    }
}
