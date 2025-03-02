using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public static class ArrayDataTypeGenerator_Cpp
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
                    writer.WriteLine(RteFunctionsGenerator_Cpp.CreateDefine(arraySizeNameMacro, arrayDT.Size.ToString() + "U", false));

                    writer.WriteLine();
                    writer.WriteLine("using " + arrayDT.Name + " = std::array<" + datatype.Name + ", " + arrayDT.Size + ">;");
                    writer.WriteLine();
                }
            }
        }
    }
}
