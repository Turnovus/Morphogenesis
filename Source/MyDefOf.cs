using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    [DefOf]
    public static class MyDefOf
    {
#pragma warning disable CS0649
        public static StatDef Turn_PawnInstability;
#pragma warning restore CS0649

        static MyDefOf() =>
            DefOfHelper.EnsureInitializedInCtor(typeof(MyDefOf));
    }
}
