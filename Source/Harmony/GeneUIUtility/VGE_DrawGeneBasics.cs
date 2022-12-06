using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using VanillaGenesExpanded;
using System.Reflection;

namespace Morphogenesis
{
    [HarmonyPatch]
    class VGE_DrawGeneBasics
    {
        static bool Prepare() =>
            ModLister.HasActiveModWithName(Utils.ModNameVFE);
        
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(
                typeof(
                    VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch),
                nameof(
                    VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch
                        .ChooseEndogeneBackground)
            );
            yield return AccessTools.Method(
                typeof(
                    VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch),
                nameof(
                    VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch
                        .ChooseXenogeneBackground)
            );
        }
        
        static void PostfixEndogeneBackground(
            ref CachedTexture __result,
            GeneDef gene
        ) =>
            __result = AuxMethods.AdjustedGeneBgFor(gene, __result);

        [HarmonyPostfix]
        static void PostfixXenogeneBackground(
            ref CachedTexture __result,
            GeneDef gene
        ) =>
            __result = AuxMethods.AdjustedGeneBgFor(gene, __result);
    }
}
