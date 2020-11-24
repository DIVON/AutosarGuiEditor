using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.DataTypes.Enum
{
    public class EnumsList : IGuidList<EnumDataType>
    {
        public override String GetName()
        {
            return "Enums";
        }
    }
}
