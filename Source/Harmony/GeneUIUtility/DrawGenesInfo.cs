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
    [HarmonyPatch(typeof(GeneUIUtility))]
    class DrawGenesInfo
    {
        static readonly MethodInfo M_BiostatsDraw =
            typeof(BiostatsTable).GetMethod(nameof(BiostatsTable.Draw));
        static readonly MethodInfo M_yMax =
            typeof(Rect).GetMethod("get_yMax");

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(GeneUIUtility.DrawGenesInfo))]
        public static IEnumerable<CodeInstruction> DrawInstabilityBiostat
        (
            IEnumerable<CodeInstruction> instructions
        )
        {
            object lastLocS = null;
            FieldInfo lastSFld = null;
            bool patchedDraw = false;
            bool patchedHeight = false;
            bool patchedY = true;
            foreach (CodeInstruction instruction in instructions)
            {
                if (!patchedHeight && instruction.opcode == OpCodes.Ldloc_2)
                {
                    patchedHeight = true;
                    // target
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_InstabilityHeightOffset);
                    yield return new CodeInstruction(OpCodes.Sub);
                }

                yield return instruction;

                if (!patchedY && instruction.opcode == OpCodes.Call &&
                    instruction.operand as MethodInfo == M_yMax)
                {
                    patchedY = true;
                    // target
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_InstabilityHeightOffset);
                    yield return new CodeInstruction(OpCodes.Sub);
                }

                if (patchedDraw) continue;

                if (instruction.opcode == OpCodes.Ldsfld)
                {
                    lastSFld = instruction.operand as FieldInfo;
                    continue;
                }

                if (instruction.opcode == OpCodes.Ldloc_S)
                {
                    lastLocS = instruction.operand;
                    continue;
                }

                if (instruction.opcode == OpCodes.Call &&
                    instruction.operand as MethodInfo == M_BiostatsDraw)
                {
                    patchedDraw = true;
                    // rect3
                    yield return new CodeInstruction(OpCodes.Ldloc_S, lastLocS);
                    // target
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    // GeneUIUtility.arc
                    yield return new CodeInstruction(OpCodes.Ldsfld, lastSFld);

                    yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_DrawInstability);
                }
            }
        }
    }
}
