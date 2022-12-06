using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;
using UnityEngine;

namespace Morphogenesis
{
    public static class AuxMethods
    {
        // Utils Methods
        public static readonly MethodInfo M_IsMutation =
            typeof(Utils).GetMethod(nameof(Utils.IsMutation));

        // PawnExtensions methods
        public static readonly MethodInfo M_HasMutation =
            typeof(PawnExtensions).GetMethod(nameof(PawnExtensions.HasMutation));

        // Pawn_GeneTracker Methods
        public static readonly MethodInfo M_Genes_RemoveGene =
            typeof(Pawn_GeneTracker).GetMethod(nameof(Pawn_GeneTracker.RemoveGene));

        // Pawn_GeneTracker Fields
        public static readonly FieldInfo F_Genes_Pawn =
            typeof(Pawn_GeneTracker).GetField(nameof(Pawn_GeneTracker.pawn));
        public static readonly FieldInfo F_Genes_Xenogenes =
            typeof(Pawn_GeneTracker).GetField("xenogenes", BindingFlags.Instance | BindingFlags.NonPublic);

        // List<Gene> Methods
        public static readonly MethodInfo M_ListGene_GetItem =
            typeof(List<Gene>).GetMethod("get_Item");

        // List<GeneDef> Methods
        public static readonly MethodInfo M_ListGeneDef_Contains =
            typeof(List<GeneDef>).GetMethod(nameof(List<GeneDef>.Contains),
                new Type[] { typeof(GeneDef) });

        // Need fields
        public static readonly FieldInfo F_Need_Pawn =
            typeof(Need).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

        // AuxMethods methods
        public static readonly MethodInfo M_AdjustedGeneBgFor =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.AdjustedGeneBgFor));
        public static readonly MethodInfo M_MaybeGetDisplayDef =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.MaybeGetDisplayDef));
        public static readonly MethodInfo M_RealGeneDefOf =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.RealGeneDefOf));
        public static readonly MethodInfo M_AdjustInstabilty =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.AdjustInstability));
        public static readonly MethodInfo M_DrawInstability =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.DrawInstability));
        public static readonly MethodInfo M_InstabilityHeightOffset =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.InstabilityHeightOffset));
        public static readonly MethodInfo M_UnmarkAsMutation =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.UnmarkAsMutation));
        public static readonly MethodInfo M_HasDefect =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.HasDefect));
        public static readonly MethodInfo M_AssemblerGenepackInstability =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.AssemblerGenepackInstability));
        public static readonly MethodInfo M_AssemblerInstability =
            typeof(AuxMethods).GetMethod(nameof(AuxMethods.AssemblerInstability));

        // GeneDefOf Fields
        public static readonly FieldInfo F_GeneDefOf_Inbred =
            typeof(GeneDefOf).GetField(nameof(GeneDefOf.Inbred));

        // CustomXenotype fields
        public static readonly FieldInfo F_Inheritable =
            typeof(CustomXenotype).GetField(nameof(CustomXenotype.inheritable));

        // Gene Fields
        public static readonly FieldInfo F_Gene_Def =
            typeof(Gene).GetField(nameof(Gene.def));

        // Gene Methods
        public static readonly MethodInfo M_Gene_GetOverridden =
             typeof(Gene).GetMethod("get_" + nameof(Gene.Overridden));

        // GeneUIUtility Methods
        public static readonly MethodInfo M_DrawBiostats =
            typeof(GeneUIUtility).GetMethod(nameof(GeneUIUtility.DrawBiostats));

        // Privately-stored private helper methods
        private static readonly MethodInfo M_MaxLabelWidth =
            typeof(BiostatsTable).GetMethod("MaxLabelWidth", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo M_DrawBiostat =
            typeof(GeneUIUtility).GetMethod("DrawStat", BindingFlags.Static | BindingFlags.NonPublic);

        public static CachedTexture AdjustedGeneBgFor(
            GeneDef gene,
            CachedTexture texturePre
        )
        {
            if (!(gene is GeneDef_Display display)) return texturePre;

            switch (display.geneType)
            {
                case GeneDef_Display.EDisplayType.Defect:
                    return Utils.DefectBackground;
                case GeneDef_Display.EDisplayType.DefectHidden:
                    return Utils.DefectDormantBackground;
                case GeneDef_Display.EDisplayType.Mutagene:
                    return Utils.MutageneBackground;
            }
            return texturePre;
        }

        public static GeneDef MaybeGetDisplayDef(Gene gene)
        {
            CompMutationTracker comp = gene.pawn?.Mutations();
            if (comp == null) return gene.def;
            if (comp.HasDefect(gene)) return
                    MutationUtils.GetDefectDefFor(gene.def);
            return comp.HasMutation(gene) ?
                MutationUtils.GetMutageneDefFor(gene.def) :
                gene.def;
        }

        public static GeneDef RealGeneDefOf(GeneDef gene)
            => gene is GeneDef_Display d ? d.original : gene;

        public static void AdjustInstability(Genepack pack, Pawn pawn)
        {
            CompInstabilityTracker comp =
                pack.GetComp<CompInstabilityTracker>();
            if (comp == null || !pawn.CanMutate())
                return;

            foreach (GeneDef gene in pack.GeneSet.GenesListForReading)
                if (pawn.Mutations().HasMutation(gene))
                    comp.baseInstability++;
        }

        public static void DrawInstability(Rect rect, Thing target, int arc)
        {
            CompInstabilityTracker comp =
                target.TryGetComp<CompInstabilityTracker>();
            if (comp == null || comp.baseInstability <= 0) return;

            double instability = comp == null ? 0 : comp.baseInstability;

            
            float textWidth = Text.CalcSize("Biostat_Instability".Translate()).x;
            textWidth = M_MaxLabelWidth.Invoke(null, new object[] { arc }) as float? ?? textWidth;
            float y = rect.y + BiostatsTable.HeightForBiostats(arc);
            float height = InstabilityHeightOffset(target);

            // Most of this is just copy-pasted from the vanilla biostat-
            // drawing method. I hate GUI.
            Rect listing = new Rect(0f, y, rect.xMax, height);
            Rect listingBg = new Rect(0f, 0f, rect.xMax, height);
            Rect icon = new Rect(0f, 4f, 22f, 22f);
            Rect text = new Rect(icon.xMax + 4f, 0, textWidth, height);
            Rect selection = new Rect(0, text.y, textWidth, height);

            GUI.BeginGroup(listing);

            if (arc  > 0)
                Widgets.DrawLightHighlight(listingBg);
            Widgets.DrawHighlightIfMouseover(listingBg);

            GUI.DrawTexture(icon, Utils.InstabilityTex.Texture);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(text, "Biostat_Instability".Translate());

            TaggedString tip = "Biostat_InstabilityDesc".Translate();
            TooltipHandler.TipRegion(selection, tip);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(textWidth + 30f, 0f, 90f, height), instability.ToString());

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        public static float InstabilityHeightOffset(Thing target)
        {
            CompInstabilityTracker comp = target.TryGetComp<CompInstabilityTracker>();
            return comp != null && comp.baseInstability > 0 ? Text.LineHeight * 1.5f : 0;
        }

        public static void UnmarkAsMutation(Pawn pawn, Gene gene)
        {
            CompMutationTracker comp = pawn.TryGetComp<CompMutationTracker>();
            if (comp != null) comp.UnmarkAsMutation(gene);
        }

        public static bool HasDefect(Pawn pawn, Gene gene)
        {
            if (!pawn.CanMutate()) return false;
            return pawn.Mutations().HasDefect(gene);
        }

        public static void AssemblerGenepackInstability
        (
            Genepack pack,
            ref float curX,
            float curY,
            float margin = 6f
        )
        {
            CompInstabilityTracker comp =
                pack?.GetComp<CompInstabilityTracker>();

            Log.Message((pack == null).ToString());
            Log.Message((comp == null).ToString());
            if (pack?.GeneSet?.GenesListForReading != null)
                foreach (GeneDef gene in pack.GeneSet.GenesListForReading)
                    Log.Message(gene.defName);

            if (pack == null || comp == null || comp.baseInstability == 0)
                return;

            Log.Message(comp.baseInstability.ToString());

            float height = GeneCreationDialogBase.GeneSize.y / 3f;
            float textHeight = Text.LineHeightOf(GameFont.Small);

            float xInt = curX - 34;
            float yInt = curY + margin + height;

            if (pack.GeneSet.MetabolismTotal != 0) yInt += margin + height;
            // Instability will draw over archite count, but that's fine since
            // instability and archites are mutually exclusive during normal
            // play.

            Rect icon = new Rect(xInt, yInt, textHeight, textHeight);
            M_DrawBiostat.Invoke(null, new object[]
                { icon, Utils.InstabilityTex, comp.baseInstability.ToString(), textHeight});
            //DrawStat(icon, Utils.InstabilityTex, comp.baseInstabilty.ToString(), textHeight);
            Rect tip = new Rect(xInt, yInt, 38f, textHeight);
            if (Mouse.IsOver(tip))
            {
                Widgets.DrawHighlight(tip);
                TooltipHandler.TipRegion(
                    tip,
                    "Biostat_Instability".Translate().Colorize(
                        ColoredText.TipSectionTitleColor) +
                    "\n\n" + "Biostat_InstabilityDesc".Translate());
            }
        }

        public static void AssemblerInstability
        (
            float instability,
            float met,
            ref float curX,
            float curY,
            float margin = 6f
        )
        {
            float height = GeneCreationDialogBase.GeneSize.y / 3f;
            float textHeight = Text.LineHeightOf(GameFont.Small);

            float yInt = curY + margin + height;

            if (met != 0) yInt += margin + height;
            // Instability will draw over archite count, but that's fine since
            // instability and archites are mutually exclusive during normal
            // play.

            Rect icon = new Rect(curX, yInt, textHeight, textHeight);
            M_DrawBiostat.Invoke(null, new object[]
                { icon, Utils.InstabilityTex, instability.ToString(), textHeight});
            //DrawStat(icon, Utils.InstabilityTex, comp.baseInstabilty.ToString(), textHeight);
            Rect tip = new Rect(curX, yInt, 38f, textHeight);
            if (Mouse.IsOver(tip))
            {
                Widgets.DrawHighlight(tip);
                TooltipHandler.TipRegion(
                    tip,
                    "Biostat_Instability".Translate().Colorize(
                        ColoredText.TipSectionTitleColor) +
                    "\n\n" + "Biostat_InstabilityDesc".Translate());
            }
        }
    }
}
