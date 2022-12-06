using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    public class MutationProps : DefModExtension
    {
#pragma warning disable CS0649
        public bool canBeMutagene = true;
        public bool canBeDefect = false;

        public float baseMutageneWeight = 1f;
        public float baseDefectWeight = 1f;

        public List<GeneDef> defectBlockedBy = new List<GeneDef>();
#pragma warning restore CS0649
    }
}
