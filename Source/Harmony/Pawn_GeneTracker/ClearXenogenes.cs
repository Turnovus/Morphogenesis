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
    class ClearXenogenes
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Pawn_GeneTracker.ClearXenogenes))]
        public static IEnumerable<CodeInstruction> ClearIgnoresMutations
        (
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator
        )
        {
            // Flag:
            // 0: Looking for Br.S to do main patch
            // 1: Removing next label, adding it to patch
            // 2: Looking for Call Pawn_GeneTracker::RemoveGene
            // 3: Adding new label to next instruction
            // 4: Patch Done
            int flag = 0;
            Label label = generator.DefineLabel();
            CodeInstruction entry = new CodeInstruction(OpCodes.Nop);
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (flag == 4) continue;

                switch (flag)
                {
                    case 0:
                        if (!(instruction.operand is Label) || instruction.opcode != OpCodes.Br_S) break;

                        flag = 1;

                        // this.pawn
                        entry = new CodeInstruction(OpCodes.Ldarg_0);
                        yield return entry;
                        yield return new CodeInstruction(OpCodes.Ldfld, AuxMethods.F_Genes_Pawn);
                        // this.xenogenes[index]
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, AuxMethods.F_Genes_Xenogenes);
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // index
                        yield return new CodeInstruction(OpCodes.Callvirt, AuxMethods.M_ListGene_GetItem);

                        yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_HasMutation);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                        break;
                    case 1:
                        if (instruction.labels.NullOrEmpty()) break;
                        
                        flag = 2;
                        foreach (Label l in instruction.labels)
                            entry.labels.Add(l);
                        instruction.labels.Clear();
                        break;
                    case 2:
                        if (instruction.opcode != OpCodes.Call && instruction.operand as MethodInfo != AuxMethods.M_Genes_RemoveGene)
                            break;
                        flag = 3;
                        break;
                    case 3:
                        flag = 4;
                        instruction.labels.Add(label);
                        break;
                }
            }

            if (flag < 4)
                Utils.Error("Transpiler ClearIgnoresMutations failed! " +
                    "Flag = " + flag.ToString());
        }
    }
}
