using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Morphogenesis
{
    [StaticConstructorOnStartup]   
    class PatchRunner
    {
        static PatchRunner()
        {
            Log.Message("Morhpogenesis starting Harmony patches...");
            Harmony harmony = new Harmony("turnovus.biotech.morphogenesis");
            harmony.PatchAll();
            Log.Message(harmony.ToString());
        }
    }
}
