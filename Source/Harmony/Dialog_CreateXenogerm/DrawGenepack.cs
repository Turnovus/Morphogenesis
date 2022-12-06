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
    [HarmonyPatch(typeof(Dialog_CreateXenogerm))]
    class DrawGenepack
    {
        [HarmonyTranspiler]
        [HarmonyPatch("DrawGenepack")]
        public static IEnumerable<CodeInstruction> DrawGenepackInstability
        (
            IEnumerable<CodeInstruction> instructions
        )
        {
            bool patched = false;
            object lastLdFld = null;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (patched) continue;
                if (instruction.opcode == OpCodes.Ldfld)
                    lastLdFld = instruction.operand;

                if (instruction.opcode != OpCodes.Call ||
                    instruction.operand as MethodInfo != AuxMethods.M_DrawBiostats)
                    continue;

                patched = true;
                // genepack
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldfld, lastLdFld);
                // curX
                yield return new CodeInstruction(OpCodes.Ldarg_2);
                // curY
                yield return new CodeInstruction(OpCodes.Ldarg_3);
                // 4f
                yield return new CodeInstruction(OpCodes.Ldc_R4, 4f);
                
                yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_AssemblerGenepackInstability);
            }
            if (!patched)
                Utils.Error("Transpiler DrawGenepackInstability Failed!");
        }
    }
}
