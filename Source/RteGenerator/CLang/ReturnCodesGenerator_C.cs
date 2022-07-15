using System;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CLang
{
    public class ReturnCodesGenerator_C
    {
        public void GenerateReturnCodesFile(String folder)
        {
            String FileName = folder + "\\" + Properties.Resources.RTE_RETURN_CODES_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, Properties.Resources.RTE_RETURN_CODES_FILENAME, "This file contains return codes of the RTE functions.");
            RteFunctionsGenerator_C.OpenGuardDefine(writer);

            writer.WriteLine("/* Rte return codes */");
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("RTE_E_OK", "0u"));
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("RTE_E_INVALID", "1u"));
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("RTE_E_LOST_DATA", "64u"));
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("RTE_E_NO_DATA", "131u"));
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("RTE_E_UNCONNECTED", "134u"));
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("RTE_E_OUT_OF_RANGE", "137u"));
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("RTE_E_LIMIT", "138u"));

            writer.WriteLine();

            RteFunctionsGenerator_C.CloseGuardDefine(writer);
            RteFunctionsGenerator_C.WriteEndOfFile(writer);
            writer.Close();
        }
    }
}
