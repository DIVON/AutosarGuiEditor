using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CMacro
{
    public static class BaseDataTypesCodeGenerator_CMacro
    {
        public static void GenerateCode(StreamWriter writer, BaseDataTypesList baseDataTypesList)
        {
            foreach (BaseDataType baseDataType in baseDataTypesList)
            {
                writer.WriteLine("typedef  " + baseDataType.SystemName + "  " + baseDataType.Name + ";" );

                ArrayDataTypeGenerator_CMacro.GenerateArrayForDataType(writer, baseDataType);
            }            
        }
    }
}
