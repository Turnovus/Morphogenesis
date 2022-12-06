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
    class DrawGenes_Infocard
    {
        static MethodInfo M_WindowStack =
            typeof(Find).GetMethod("get_WindowStack");

        [HarmonyTranspiler]
        [HarmonyPatch("DrawGeneBasics")]
        static IEnumerable<CodeInstruction> DoNotShowDisplayGeneInfo
        (
            IEnumerable<CodeInstruction> instructions
        )
        {
            // Flag:
            // 0 - Looking for Find.WindowStack
            // 1 - Looking for ldarg.0 to apply patch
            // 2 - Patch done
            int flag = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                switch (flag)
                {
                    case 0:
                        if (instruction.operand as MethodInfo == M_WindowStack)
                            flag = 1;
                        break;
                    case 1:
                        if (instruction.opcode == OpCodes.Ldarg_0)
                        {
                            flag = 2;
                            yield return new CodeInstruction(
                                OpCodes.Call, AuxMethods.M_RealGeneDefOf);
                        }
                        break;
                }
            }
        }
    }
}
