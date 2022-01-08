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
            WriteAllErrors(writer);
            RteFunctionsGenerator.CloseGuardDefine(writer);
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        void WriteErrorsCount(StreamWriter writer)
        {
            int immediateSafeStateCount = AutosarApplication.GetInstance().SystemErrors.ErrorCount(SystemErrorStrictness.ImmediateSafeState);
            int totalErrorCount =  AutosarApplication.GetInstance().SystemErrors.Count;
            writer.WriteLine(RteFunctionsGenerator.CreateDefine("SYSTEM_ERRORS_COUNT", totalErrorCount.ToString()));
            writer.WriteLine(RteFunctionsGenerator.CreateDefine("IMMEDIATE_SAFE_STATE_ERROR_COUNT", immediateSafeStateCount.ToString()));
            writer.WriteLine();
        }

        void WriteAllErrors(StreamWriter writer)
        {
            int maxLen = 0;
            

            SystemErrorsList errList = AutosarApplication.GetInstance().SystemErrors;

            foreach (SystemErrorObject err in errList)
            {
                maxLen = maxLen < err.Name.Length ?  err.Name.Length : maxLen;
            }

            maxLen += 20; /* + #define and ERR_IR_ length */

            int errCount = 0;

            writer.WriteLine("/* Immediate Safe State errors */");
            /* Write only immediate safe safe state errors */
            for (int i = 0; i < errList.Count; i++)
            {
                if (errList[i].Strictness == SystemErrorStrictness.ImmediateSafeState)
                {
                    writer.WriteLine(RteFunctionsGenerator.CreateDefine("ERR_ID_" + errList[i].Name, errCount.ToString() + "u", false, maxLen));
                    errCount++;
                }                
            }

            writer.WriteLine();
            writer.WriteLine("/* No Restriction errors */");
            /* Write left errors */
            for (int i = 0; i < errList.Count; i++)
            {
                if (errList[i].Strictness == SystemErrorStrictness.NoRestriction)
                {
                    writer.WriteLine(RteFunctionsGenerator.CreateDefine("ERR_ID_" + errList[i].Name, errCount.ToString() + "u", false, maxLen));
                    errCount++;
                }
            }
            writer.WriteLine();
        }
    }
}
