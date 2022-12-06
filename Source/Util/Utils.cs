using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    public static class Utils
    {
        public static readonly string ModNameVFE =
            "Vanilla Expanded Framework";

        public static readonly CachedTexture MutageneBackground =
            new CachedTexture("UI/Icons/Genes/GeneBackground_Morphogene");
        public static readonly CachedTexture DefectBackground =
            new CachedTexture("UI/Icons/Genes/GeneBackground_Defect");
        public static readonly CachedTexture DefectDormantBackground =
            new CachedTexture("UI/Icons/Genes/GeneBackground_DefectDormant");

        public static readonly CachedTexture InstabilityTex =
            new CachedTexture("UI/Icons/Biostats/Instability");

        public static bool IsMutation(GeneDef gene) => gene is GeneDef_Display;

        public static void Error(string message) =>
            Log.Error("Morphogenesis: " + message);
    }
}
