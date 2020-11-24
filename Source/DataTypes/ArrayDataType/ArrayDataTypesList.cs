using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.DataTypes.ArrayDataType
{
    public class ArrayDataTypesList : IGuidList<ArrayDataType>
    {
        public override String GetName()
        {
            return "Array data types";
        }

        public override Boolean CreateRoot()
        {
            return true;
        }
    }
}
