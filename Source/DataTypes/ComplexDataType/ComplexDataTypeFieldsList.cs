using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.DataTypes.ComplexDataType
{
    public class ComplexDataTypeFieldsList : IGuidList<ComplexDataTypeField>
    {
        public override String GetName()
        {
            return "Fields";
        }

        public override Boolean CreateRoot()
        {
            return false;
        }
    }
}
