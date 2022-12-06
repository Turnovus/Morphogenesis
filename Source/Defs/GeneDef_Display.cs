using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;

namespace Morphogenesis
{
    public class GeneDef_Display : GeneDef
    {
        public GeneDef original;
        public EDisplayType geneType;

        public GeneDef_Display(GeneDef def, EDisplayType type)
        {
            foreach (FieldInfo field in typeof(GeneDef).GetFields())
                field.SetValue(this, field.GetValue(def));
            defName = def.defName + "_MutationDisplayDef";
            description = "This is not a real GeneDef.\n\n" +
                "If you are reading this, then there is an issue with the" +
                "mod \"Morphogenesis\". Please report this.";
            original = def;
            geneType = type;
            if (type == EDisplayType.Defect || type == EDisplayType.DefectHidden)
                biostatMet = 0;
        }

        public enum EDisplayType
        {
            Mutagene,
            Defect,
            DefectHidden,
        }
    }
}
