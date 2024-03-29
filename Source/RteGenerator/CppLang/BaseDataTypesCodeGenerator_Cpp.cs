﻿using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public static class BaseDataTypesCodeGenerator_Cpp
    {
        public static void GenerateCode(StreamWriter writer, BaseDataTypesList baseDataTypesList)
        {
            foreach (BaseDataType baseDataType in baseDataTypesList)
            {
                if (baseDataType.Name == "boolean")
                {
                    continue;
                }
                writer.WriteLine("typedef  " + baseDataType.SystemName + "  " + baseDataType.Name + ";");

                ArrayDataTypeGenerator_Cpp.GenerateArrayForDataType(writer, baseDataType);
            }            
        }
    }
}
