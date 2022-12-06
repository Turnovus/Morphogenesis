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
using UnityEngine;

namespace Morphogenesis
{
    [HarmonyPatch(typeof(GeneUtility))]
    class ReimplantXenogerm
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(GeneUtility.ReimplantXenogerm))]
        public static IEnumerable<CodeInstruction> NeverReimplantMutations
        (
            IEnumerable<CodeInstruction> instructions
        )
        {
            // Flag:
            // 0 - looking for local var xenogene
            // 1 - looking for pop to attach label
            // 2 - patch done
            int flag = 0;
            Label label = new Label();
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                switch (flag)
                {
                    case 0:
                        if (instruction.operand != null && instruction.operand is Label)
                            label = (Label)instruction.operand;
                        else if (instruction.opcode == OpCodes.Stloc_1)
                        {
                            flag = 1;
                            //caster
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            // xenogene
                            yield return new CodeInstruction(OpCodes.Ldloc_1);
                            yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_HasMutation);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                            // The below instructions will make added xenogenes
                            // un-mutate if the recipient has them as mutations
                            // But this is unnecessary, since we've patched
                            // Pawn_GeneTracker.AddGene already.
                            // recipient
                            //yield return new CodeInstruction(OpCodes.Ldarg_1);
                            // xenogene
                            //yield return new CodeInstruction(OpCodes.Ldloc_1);
                            //yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_UnmarkAsMutation);
                        }
                        break;
                    case 1:
                        if (instruction.opcode == OpCodes.Ldloca_S)
                        {
                            flag = 2;
                            instruction.labels.Add(label);
                        }
                        break;
                }
            }
        }
    }
}
