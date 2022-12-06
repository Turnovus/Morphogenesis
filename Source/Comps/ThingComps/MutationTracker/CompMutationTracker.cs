using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    public class CompMutationTracker : ThingComp
    {
        private List<Mutagene> mutagenesInt = new List<Mutagene>();
        private List<Defect> defectsInt = new List<Defect>();
        private List<DefectHidden> defectsHiddenInt = new List<DefectHidden>();
        private bool defectsExpired = false;
        private int? metFromDefectsInt;


        public abstract class BaseMorphogene
        {
            public CompMutationTracker parent;
            public GeneDef def;
            public bool analyzed = false;
            public int ticksLeft;

            public virtual void Analyze() => analyzed = true;

            public virtual void Expire() { }

            public virtual void Tick()
            {
                if (ticksLeft > -1)
                    ticksLeft--;
                if (ticksLeft == 0)
                    Expire();
            }
        }

        public class Mutagene : BaseMorphogene
        {
            public override void Expire()
            {
                base.Expire();
                parent.Pawn.genes.RemoveGene(
                    parent.Pawn.genes.GetGene(def));
            }
        }

        public class Defect : BaseMorphogene
        {
        }

        public class DefectHidden : BaseMorphogene
        {
            public override void Expire()
            {
                base.Expire();
                if (parent.Pawn.genes.HasGene(def)) return;
                Gene gene = parent.Pawn.genes.AddGene(def, true);
                parent.MarkAsDefect(gene);
            }
        }

        public CompProperties_MutationTracker Props =>
            (CompProperties_MutationTracker)props;

        public Pawn Pawn => parent as Pawn;

        public int MetFromDefects
        {
            get
            {
                if (metFromDefectsInt == null)
                {
                    metFromDefectsInt = 0;
                    foreach (Defect d in defectsInt)
                    {
                        Gene gene = Pawn.genes.GetGene(d.def);
                        if (gene != null && gene.Active)
                            metFromDefectsInt += d.def.biostatMet;
                    }
                }

                return (int)metFromDefectsInt;
            }
        }

        public bool HasMutation(GeneDef gene)
            => HasMutagene(gene) || HasDefect(gene);

        public bool HasMutation(Gene gene)
            => HasMutation(gene.def);

        public bool HasMutagene(GeneDef gene)
            => mutagenesInt.Any(x => x.def == gene);

        public bool HasMutagene(Gene gene)
            => HasMutagene(gene.def);

        public bool HasDefect(Gene gene)
            => HasDefect(gene.def);

        public bool HasDefect(GeneDef gene)
            => defectsInt.Any(x => x.def == gene);

        public bool HasHiddenDefect(Gene gene)
            => HasHiddenDefect(gene.def);

        public bool HasHiddenDefect(GeneDef gene)
        {
            foreach (DefectHidden defect in defectsHiddenInt)
                if (defect.def == gene) return true;
            return false;
        }

        public void MarkAsMutation(Gene gene, int ticks=GenDate.TicksPerDay)
        {
            MarkAsMutation(gene.def, ticks);
        }

        public void MarkAsMutation(GeneDef gene, int ticks = GenDate.TicksPerDay)
        {
            if (!HasMutation(gene))
                mutagenesInt.Add(new Mutagene()
                    { parent = this, def = gene, ticksLeft = ticks });
        }

        public void MarkAsDefect(Gene gene)
            => MarkAsDefect(gene.def);

        public void MarkAsDefect(GeneDef gene)
        {
            if (HasMutation(gene)) return;
            defectsInt.Add(new Defect() { parent = this, def = gene });
        }

        public void UnmarkAsMutation(Gene gene) => UnmarkAsMutation(gene.def);

        public void UnmarkAsMutation(GeneDef gene)
        {
            List<Mutagene> list = new List<Mutagene>();
            foreach (Mutagene m in mutagenesInt)
                if (m.def != gene)
                    list.Add(m);
            mutagenesInt = list;

            List<Defect> list2 = new List<Defect>();
            foreach (Defect d in defectsInt)
                if (d.def != gene)
                    list2.Add(d);
            defectsInt = list2;

            defectsHiddenInt = GetStillValidHiddenDefects();
        }

        public Gene AddMutation(GeneDef geneDef, int ticks = GenDate.TicksPerDay)
        {
            if (Pawn == null)
            {
                Utils.Error("Attempted to mutate non-pawn: " +
                    parent.ToString());
                return null;
            }
            Gene gene = Pawn.genes.AddGene(geneDef, true);
            if (gene != null)
                MarkAsMutation(gene, ticks);
            return gene;
        }

        public void Notify_GenesChanged(GeneDef gene)
        {
            mutagenesInt = GetStillValidMutagenes();
            defectsInt = GetStillValidDefects();
            defectsHiddenInt = GetStillValidHiddenDefects();
            metFromDefectsInt = null;
        }

        public bool CanAcceptDefect(GeneDef defect)
        {
            if (!MutationUtils.CanBeDefect(defect)) return false;

            MutationProps mutation = defect.Mutations();

            if (HasHiddenDefect(defect)) return false;

            if (!mutation.defectBlockedBy.NullOrEmpty())
                foreach (GeneDef otherDef in mutation.defectBlockedBy)
                    if (otherDef == defect) return false;

            if (defect.prerequisite != null &&
                !Pawn.genes.HasGene(defect.prerequisite))
                return false;
            return true;
        }

        public void AddHiddenDefect(GeneDef gene, int ticks = GenDate.TicksPerDay)
        {
            if (!MutationUtils.CanBeDefect(gene)) return;
            defectsHiddenInt.Add(new DefectHidden()
                { parent = this, def = gene, ticksLeft = ticks });
        }

        public void ExpireDormantDefects()
            => defectsExpired = true;

        public void ExpireAllDormantDefects()
        {
            foreach (DefectHidden d in defectsHiddenInt) d.Expire();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref mutagenesInt, "mutationsInt", LookMode.Deep);
            Scribe_Collections.Look(ref defectsInt, "defectsInt", LookMode.Deep);
            Scribe_Collections.Look(ref defectsHiddenInt, "defectsHiddenInt", LookMode.Deep);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (defectsExpired)
            {
                defectsHiddenInt = GetStillValidHiddenDefects();
                metFromDefectsInt = null;
                defectsExpired = false;
            }

            foreach (Mutagene i in mutagenesInt) i.Tick();
            foreach (Defect j in defectsInt) j.Tick();
            foreach (DefectHidden k in defectsHiddenInt) k.Tick();
        }

        private List<Mutagene> GetStillValidMutagenes()
        {
            List<Mutagene> list = new List<Mutagene>();
            foreach (Mutagene m in mutagenesInt)
                if (Pawn.genes.HasGene(m.def))
                    list.Add(m);
            return list;
        }

        private List<Defect> GetStillValidDefects()
        {
            List<Defect> list = new List<Defect>();
            foreach (Defect d in defectsInt)
                if (Pawn.genes.HasGene(d.def))
                    list.Add(d);
            return list;
        }

        private List<DefectHidden> GetStillValidHiddenDefects()
        {
            List<DefectHidden> list = new List<DefectHidden>();
            foreach (DefectHidden d in defectsHiddenInt)
                if (d.ticksLeft > 0 && !Pawn.genes.HasGene(d.def))
                    list.Add(d);
            return list;
        }
    }
}
