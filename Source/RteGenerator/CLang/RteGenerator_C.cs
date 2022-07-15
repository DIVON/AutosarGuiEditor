using System;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CLang
{
    public class RteGenerator_C
    {
        public bool Generate()
        {
            /* Create base folders */
            Directory.CreateDirectory(RteFunctionsGenerator_C.GetRteFolder());
            GenerateDataTypesFile(RteFunctionsGenerator_C.GetRteFolder());
            GenerateScheduler();

            RteConnectionGenerator_C connectionsGenerator = new RteConnectionGenerator_C();
            connectionsGenerator.GenerateConnections(RteFunctionsGenerator_C.GetRteFolder());

            ReturnCodesGenerator_C returnCodesGenerator = new ReturnCodesGenerator_C();
            returnCodesGenerator.GenerateReturnCodesFile(RteFunctionsGenerator_C.GetRteFolder());

            /* Create system errors file */
            SystemErrorGenerator_C systemErrorGenerator = new SystemErrorGenerator_C();
            systemErrorGenerator.GenerateSystemErrorsFile(RteFunctionsGenerator_C.GetRteFolder());

            GenerateComponentsFiles();
            
            return true;
        }

       
        void GenerateDataTypesFile(String folder)
        {
            RteDataTypesGenerator_C dataTypesGenerator = new RteDataTypesGenerator_C();
            dataTypesGenerator.GenerateDataTypesFile(folder);
        }

        
        public void GenerateComponentsFiles()
        {
            Directory.CreateDirectory(RteFunctionsGenerator_C.GetComponentsFolder());

            RteComponentGenerator_C componentGenerator = new RteComponentGenerator_C();
            componentGenerator.GenerateComponentsFiles();
        }
        

        void GenerateScheduler()
        {
            RteSchedulerGenerator_C schedulerGenerator = new RteSchedulerGenerator_C();
            schedulerGenerator.GenerateShedulerFiles(RteFunctionsGenerator_C.GetRteFolder());
        }
    }
}
