using System;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public class ReturnCodesGenerator_Cpp
    {
        public void GenerateReturnCodesFile(String folder)
        {
            String FileName = folder + "\\" + Properties.Resources.RTE_RETURN_CODES_HPP_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, Properties.Resources.RTE_RETURN_CODES_FILENAME, "This file contains return codes of the RTE functions.");
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);

            writer.WriteLine("/* Rte return codes */");
            writer.WriteLine(" enum class Std_ReturnType");
            writer.WriteLine("{");
            writer.WriteLine("    RTE_E_OK,");
            writer.WriteLine("    RTE_E_INVALID,");
            writer.WriteLine("    RTE_E_LOST_DATA,");
            writer.WriteLine("    RTE_E_NO_DATA,");
            writer.WriteLine("    RTE_E_UNCONNECTED,");
            writer.WriteLine("    RTE_E_OUT_OF_RANGE,");
            writer.WriteLine("    RTE_E_LIMIT,");
            writer.WriteLine("    RTE_E_ERROR");
            writer.WriteLine("};");

            writer.WriteLine();

            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);
            writer.Close();
        }
    }
}
