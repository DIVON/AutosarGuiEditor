using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class ReturnCodesGenerator
    {
        public void GenerateReturnCodesFile(String folder)
        {
            String FileName = folder + "\\" + Properties.Resources.RTE_RETURN_CODES_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator.GenerateFileTitle(writer, Properties.Resources.RTE_RETURN_CODES_FILENAME, "This file contains return codes of the RTE functions.");
            String guardDefine = RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine("/* Rte return codes */");
            writer.WriteLine(RteFunctionsGenerator.CreateDefine(Properties.Resources.RTE_E_OK, "((uint32)0xFF)"));
            writer.WriteLine(RteFunctionsGenerator.CreateDefine(Properties.Resources.RTE_E_UNCONNECTED, "((uint32)0xFE)"));
            writer.WriteLine();

            RteFunctionsGenerator.CloseGuardDefine(writer);
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }
    }
}
