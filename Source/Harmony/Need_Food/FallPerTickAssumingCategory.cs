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
    [HarmonyPatch(typeof(Need_Food))]
    class FallPerTickAssumingCategory
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Need_Food.FoodFallPerTickAssumingCategory))]
        public static IEnumerable<CodeInstruction> MetabolismIgnoresDefects
        (
            IEnumerable<CodeInstruction> instructions
        )
        {
            // Log.Message(AuxMethods.F_Need_Pawn.ToString());
            // Flag:
            // 0 - Looking for Gene.Overridden
            // 1 - Looking for next label to do patch
            // 2 - Patch done
            int flag = 0;
            // This will contain the currently evaluated gene once we're ready
            // to patch
            object lastStLocS = null;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (flag == 2) continue;

                if (instruction.opcode == OpCodes.Stloc_S)
                    lastStLocS = instruction.operand;

                switch (flag)
                {
                    case 0:
                        if (instruction.opcode == OpCodes.Callvirt &&
                            instruction.operand as MethodInfo == AuxMethods.M_Gene_GetOverridden)
                            flag = 1;
                        break;
                    case 1:
                        if (!(instruction.operand is Label label)) break;
                        flag = 2;

                        // this
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        // this.pawn
                        yield return new CodeInstruction(OpCodes.Ldfld, AuxMethods.F_Need_Pawn);
                        // gene
                        yield return new CodeInstruction(OpCodes.Ldloc_S, lastStLocS);

                        yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_HasDefect);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                        
                        break;
                }
            }
            if (flag != 2)
                Utils.Error("Transpiler MetabolismIgnoresDefects Failed! " +
                    "Flag = " + flag.ToString());
        }
    }
}
