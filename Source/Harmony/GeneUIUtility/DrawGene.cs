using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;

namespace Morphogenesis
{
    [HarmonyPatch(typeof(GeneUIUtility))]
    class DrawGene
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(GeneUIUtility.DrawGene))]
        public static IEnumerable<CodeInstruction> UseDisplayGeneIfMutation
        (
            IEnumerable<CodeInstruction> instructions
        )
        {
            bool patched = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (patched || instruction.opcode != OpCodes.Ldfld)
                {
                    yield return instruction;
                    continue;
                }
                patched = true;
                instruction.opcode = OpCodes.Call;
                instruction.operand = AuxMethods.M_MaybeGetDisplayDef;
                yield return instruction;
            }
            if (!patched)
                Utils.Error(
                    "Failed to run UseDisplayGeneIfMutation transpiler!");
        }
    }
}