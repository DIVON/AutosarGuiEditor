using AutosarGuiEditor.Source.Autosar.SystemErrors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class SystemErrorGenerator
    {
        public SystemErrorGenerator()
        {
        }

        public void GenerateSystemErrorsFile(String folder)
        {
            String FileName = folder + "\\" + Properties.Resources.SYSTEM_ERRORS_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator.GenerateFileTitle(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME, Properties.Resources.DATATYPES_H_FILE_DESCRIPTION);
            String guardDefine = RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine("/* Rte return codes */");
            writer.WriteLine(RteFunctionsGenerator.CreateDefine(Properties.Resources.RTE_E_OK, "((uint32)0xFF)"));
            writer.WriteLine(RteFunctionsGenerator.CreateDefine(Properties.Resources.RTE_E_UNCONNECTED, "((uint32)0xFE)"));
            writer.WriteLine();

            writer.WriteLine("/*  System errors */");
            WriteErrorsCount(writer);
            writer.WriteLine();
            WriteAllErrors(writer);
            writer.WriteLine();
            RteFunctionsGenerator.CloseGuardDefine(writer);
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        void WriteErrorsCount(StreamWriter writer)
        {            
            writer.WriteLine(RteFunctionsGenerator.CreateDefine("SYSTEM_ERRORS_COUNT", AutosarApplication.GetInstance().SystemErrors.Count.ToString()));   
        }

        void WriteAllErrors(StreamWriter writer)
        {
            AutosarApplication.GetInstance().SystemErrors.SortErrorsByID();
            foreach (SystemErrorObject obj in AutosarApplication.GetInstance().SystemErrors)
            {
                writer.WriteLine(RteFunctionsGenerator.CreateDefine(obj.Name, obj.Value.ToString()));   
            }            
        }
    }
}
