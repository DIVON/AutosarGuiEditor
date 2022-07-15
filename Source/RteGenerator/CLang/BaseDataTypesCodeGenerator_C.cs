using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CLang
{
    public static class BaseDataTypesCodeGenerator_C
    {
        public static void GenerateCode(StreamWriter writer, BaseDataTypesList baseDataTypesList)
        {
            foreach (BaseDataType baseDataType in baseDataTypesList)
            {
                writer.WriteLine("typedef  " + baseDataType.SystemName + "  " + baseDataType.Name + ";" );

                ArrayDataTypeGenerator_C.GenerateArrayForDataType(writer, baseDataType);
            }            
        }
    }
}
