using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CLang
{
    public static class ArrayDataTypeGenerator_C
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
                    writer.WriteLine(RteFunctionsGenerator_C.CreateDefine(arraySizeNameMacro, arrayDT.Size.ToString() + "U", false));

                    writer.WriteLine();
                    writer.WriteLine("typedef " + datatype.Name + " " + arrayDT.Name + "[" + arrayDT.Size + "];");
                    writer.WriteLine();
                }
            }
        }
    }
}
