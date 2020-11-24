using AutosarGuiEditor.Source.RteGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.DataTypes.BaseDataType
{
    public static class BaseDataTypesCodeGenerator
    {
        public static void GenerateCode(StreamWriter writer, BaseDataTypesList baseDataTypesList)
        {
            foreach (BaseDataType baseDataType in baseDataTypesList)
            {
                writer.WriteLine("typedef  " + baseDataType.SystemName + "  " + baseDataType.Name + ";" );

                ArrayDataTypeGenerator.GenerateArrayForDataType(writer, baseDataType);
            }            
        }
    }
}
