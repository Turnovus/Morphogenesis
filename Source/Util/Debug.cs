using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    class Debug
    {
        [DebugAction(
            "Morphogenesis",
            null,
            allowedGameStates = AllowedGameStates.PlayingOnMap
        )]
        private static List<DebugActionNode> AddMutation()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();
            foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs
                .OrderBy(x => x.defName))
            {
                GeneDef localDef = gene;
                if (!MutationUtils.CanBeMutation(localDef))
                    continue;
                list.Add(new DebugActionNode(
                    localDef.defName, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = p =>
                    {
                        if (p.CanMutate())
                            p.Mutations()?.AddMutation(localDef);
                        DebugActionsUtility.DustPuffFrom(p);
                    }
                });
            }
            return list;
        }

        [DebugAction(
            "Morphogenesis",
            null,
            allowedGameStates = AllowedGameStates.PlayingOnMap
        )]
        private static void MakeUnstableGenepack()
        {
            IntVec3 c = UI.MouseCell();
            FleckMaker.ThrowMetaPuff(c.ToVector3Shifted(), Find.CurrentMap);
            Thing thing = ThingMaker.MakeThing(ThingDefOf.Genepack);

            CompInstabilityTracker comp = thing.TryGetComp<CompInstabilityTracker>();
            if (comp != null) comp.baseInstability = 1;

            GenPlace.TryPlaceThing(thing, c, Find.CurrentMap, ThingPlaceMode.Near);
        }

        [DebugAction(
            "Morphogenesis",
            null,
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap
        )]
        public static void GiveRandomDefect(Pawn p)
        {
            DebugActionsUtility.DustPuffFrom(p);
            if (!p.CanMutate()) return;

            p.Mutations().AddHiddenDefect(MutationUtils.GetAnyDefectFor(p));
        }

        [DebugAction(
            "Morphogenesis",
            null,
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap
        )]
        public static void ActivateAllDefects(Pawn p)
        {
            DebugActionsUtility.DustPuffFrom(p);
            if (!p.CanMutate()) return;

            p.Mutations().ExpireAllDormantDefects();
        }
    }
}
