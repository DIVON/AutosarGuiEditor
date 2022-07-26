using AutosarGuiEditor.Source.Autosar.SystemErrors;
using System;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CLang
{
    public class SystemErrorGenerator_C
    {
        public SystemErrorGenerator_C()
        {
        }

        public void GenerateSystemErrorsFile(String folder)
        {
            String FileName = folder + "\\" + Properties.Resources.SYSTEM_ERRORS_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, Properties.Resources.SYSTEM_ERRORS_H_FILENAME, "This file contains all defined system error IDs");
            RteFunctionsGenerator_C.OpenGuardDefine(writer);

            writer.WriteLine("/*  System errors */");
            WriteErrorsCount(writer);
            WriteAllErrors(writer);
            RteFunctionsGenerator_C.CloseGuardDefine(writer);
            RteFunctionsGenerator_C.WriteEndOfFile(writer);
            writer.Close();
        }

        void WriteErrorsCount(StreamWriter writer)
        {
            int immediateSafeStateCount = AutosarApplication.GetInstance().SystemErrors.ErrorCount(SystemErrorStrictness.ImmediateSafeState);
            int totalErrorCount =  AutosarApplication.GetInstance().SystemErrors.Count;
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("SYSTEM_ERRORS_COUNT", (totalErrorCount + 1).ToString()));
            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("IMMEDIATE_SAFE_STATE_ERROR_COUNT", immediateSafeStateCount.ToString()));
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

            int errCount = 1;

            writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("ERR_ID_INCORRECT_ERROR_ID", "0u", false, maxLen));
            writer.WriteLine();
            writer.WriteLine("/* Immediate Safe State errors */");
            /* Write only immediate safe safe state errors */
            for (int i = 0; i < errList.Count; i++)
            {
                if (errList[i].Strictness == SystemErrorStrictness.ImmediateSafeState)
                {
                    writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("ERR_ID_" + errList[i].Name, errCount.ToString() + "u", false, maxLen));
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
                    writer.WriteLine(RteFunctionsGenerator_C.CreateDefine("ERR_ID_" + errList[i].Name, errCount.ToString() + "u", false, maxLen));
                    errCount++;
                }
            }
            writer.WriteLine();
        }
    }
}
