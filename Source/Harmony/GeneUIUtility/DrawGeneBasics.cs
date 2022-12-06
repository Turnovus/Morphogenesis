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
    class DrawGeneBasics
    {
        static FieldInfo GeneBGField =
            typeof(GeneUIUtility).GetField(
                "GeneBackground_Xenogene",
                BindingFlags.NonPublic | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> BackgroundInstructions(
            object operand,
            List<Label> labels
        )
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0) //gene
                { labels = labels };
            yield return new CodeInstruction(OpCodes.Ldloc_S, operand);
            yield return new CodeInstruction(OpCodes.Call, AuxMethods.M_AdjustedGeneBgFor);
            yield return new CodeInstruction(OpCodes.Stloc_S, operand);
        }

        // Don't patch anything if VFE is installed. We'll handle compatibility
        // by patching their methods.
        static bool Prepare() =>
            !ModLister.HasActiveModWithName(Utils.ModNameVFE);

        [HarmonyTranspiler]
        [HarmonyPatch("DrawGeneBasics")]
        static IEnumerable<CodeInstruction> MutationBackgroundSupport
        (
            IEnumerable<CodeInstruction> instructions
        )
        {
            // Flag:
            // 0 - looking for xenogene texture reference
            // 1 - looking for line that stores bg's cached texture
            // 2 - looking for end of switch statement
            // 3 - done
            int flag = 0;
            object textureOperand = null;
            List<Label> postSwitch = new List<Label>();
            List<CodeInstruction> beforePatch =
                new List<CodeInstruction>();
            List<CodeInstruction> afterPatch =
                new List<CodeInstruction>();
            foreach (CodeInstruction instruction in instructions)
            {
                if (flag < 2)
                    beforePatch.Add(instruction);
                else
                    afterPatch.Add(instruction);

                switch (flag)
                {
                    case 0:
                        if (instruction.operand as FieldInfo == GeneBGField)
                            flag = 1;
                        break;
                    case 1:
                        if (instruction.opcode == OpCodes.Stloc_S)
                        {
                            textureOperand = instruction.operand;
                            flag = 2;
                        }
                        break;
                    case 2:
                        if (!instruction.labels.NullOrEmpty() && instruction.labels.Count > 0)
                        {
                            foreach (Label label in instruction.labels)
                                postSwitch.Add(label);
                            instruction.labels.Clear();
                            flag = 3;
                        }
                        break;
                }
            }
            if (flag != 3)
                Utils.Error("DrawGenesBasic transpiler failed. " +
                    "Flag: " + flag.ToString());

            //Log.Message(postSwitch.ToString());
            //foreach (Label l in postSwitch) Log.Message(l.ToString());

            foreach (CodeInstruction i in beforePatch)
            {
                yield return i;
                //Log.Message(i.ToString());
            }
            //Log.Message("---- Patch Start ----");
            foreach (CodeInstruction i in BackgroundInstructions(textureOperand, postSwitch))
            {
                yield return i;
                //Log.Message(i.ToString());
            }
            //Log.Message("---- Patch End ----");
            foreach (CodeInstruction i in afterPatch)
            {
                yield return i;
                //Log.Message(i.ToString());
            }
        }
    }
}
