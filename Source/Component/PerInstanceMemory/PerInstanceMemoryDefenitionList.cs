using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Component.PerInstanceMemory
{
    public class PerInstanceMemoryDefenitionList : IGuidList<PimDefenition>
    {
        public override String GetName()
        {
            return "Pim defenitions";
        }
    }
}
