using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public static class ArrayDataTypeGenerator
    {
        static ArrayDataType GetArrayDatatypeForDatatype(IGUID datatype)
        {
            foreach (ArrayDataType arrayDT in AutosarApplication.GetInstance().ArrayDataTypes)
            {
                if (arrayDT.DataTypeGUID.Equals(datatype))
                {
                    return arrayDT;
                }
            }
            return null;
        }
        
        public static void GenerateArrayForDataType(StreamWriter writer, IGUID datatype)
        {
            foreach (ArrayDataType arrayDT in AutosarApplication.GetInstance().ArrayDataTypes)
            {
                if (arrayDT.DataTypeGUID.Equals(datatype.GUID))
                {
                    /* Write array size */
                    String arraySizeNameMacro = arrayDT.Name + "_ELEMENTS_COUNT";
                    writer.WriteLine(RteFunctionsGenerator.CreateDefine(arraySizeNameMacro, arrayDT.Size.ToString() + "U", false));

                    writer.WriteLine();
                    writer.WriteLine("typedef " + datatype.Name + " " + arrayDT.Name + "[" + arrayDT.Size + "];");
                    writer.WriteLine();
                }
            }
        }
    }
}
