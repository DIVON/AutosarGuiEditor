using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Component.CData
{
    public class CDataDefenitionList : IGuidList<CDataDefenition>
    {
        public override String GetName()
        {
            return "CData defenitions";
        }
    }
}
