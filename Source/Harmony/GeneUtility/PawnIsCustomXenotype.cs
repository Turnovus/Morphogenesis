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
    [HarmonyPatch(typeof(GeneUtility))]
    class PawnIsCustomXenotype
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(GeneUtility.PawnIsCustomXenotype))]
        public static IEnumerable<CodeInstruction> XenotypesNeverCountMutations
        (
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator
        )
        {
            // At this point, we know:
            // - pawn has the gene
            // - gene can pass on directly (i.e. is not inbred)
            // - custom xenotype does not share the gene
            // There is a return method after this patch. If we do not jump
            // past it, then the method returns false.

            // Flag:
            // 0 - Looking for List<GeneDef>.contains
            // 1 - Looking for next label operand to do patch
            // 2 - Patch done
            int flag = 0;
            // By the time we're ready to patch, this will contain the field of
            // the loop's index
            object lastStLoc = null;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (flag >= 2) continue;

                if (instruction.opcode == OpCodes.Stloc_S)
                    lastStLoc = instruction.operand;

                switch (flag)
                {
                    case 0:
                        if (instruction.opcode == OpCodes.Callvirt &&
                            instruction.operand as MethodInfo == AuxMethods.M_ListGeneDef_Contains)
                            flag = 1;
                        break;
                    case 1:
                        if (!(instruction.operand is Label label)) break;

                        // Jumping to this label will cause the method to
                        // return false
                        Label ifHeritable = generator.DefineLabel();

                        flag = 2;

                        // If custom xenogerm is heritable, return false
                        // custom.inheritable
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldfld, AuxMethods.F_Inheritable);

                        yield return new CodeInstruction(OpCodes.Brtrue, ifHeritable);

                        // If pawn has gene as a mutation, DO NOT return false
                        // pawn
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        // list
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        // index
                        yield return new CodeInstruction(OpCodes.Ldloc_S, lastStLoc);
                        // list[index]
                        yield return new CodeInstruction(OpCodes.Callvirt, AuxMethods.M_ListGene_GetItem);

                        // PawnExtensions.HasMutation(pawn, list[index])
                        yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_HasMutation);
                        yield return new CodeInstruction(OpCodes.Brtrue, label);

                        // Jump here to return false
                        yield return new CodeInstruction(OpCodes.Nop)
                        { labels = new List<Label> { ifHeritable } };

                        break;
                }
            }
        }
    }
}
