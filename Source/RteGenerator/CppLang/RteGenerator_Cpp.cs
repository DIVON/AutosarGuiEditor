using System;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public class RteGenerator_Cpp
    {
        public bool Generate()
        {
            /* Create base folders */
            Directory.CreateDirectory(RteFunctionsGenerator_Cpp.GetRteFolder());
            GenerateDataTypesFile(RteFunctionsGenerator_Cpp.GetRteFolder());
            GenerateScheduler();

            RteConnectionGenerator_Cpp connectionsGenerator = new RteConnectionGenerator_Cpp();
            connectionsGenerator.GenerateConnections(RteFunctionsGenerator_Cpp.GetRteFolder());

            ReturnCodesGenerator_Cpp returnCodesGenerator = new ReturnCodesGenerator_Cpp();
            returnCodesGenerator.GenerateReturnCodesFile(RteFunctionsGenerator_Cpp.GetRteFolder());

            RteObjectConstructorGenerator_Cpp constructorsGenerator = new RteObjectConstructorGenerator_Cpp();
            constructorsGenerator.GenerateConstructosFile(RteFunctionsGenerator_Cpp.GetRteFolder());

            /* Create system errors file */
            SystemErrorGenerator_Cpp systemErrorGenerator = new SystemErrorGenerator_Cpp();
            systemErrorGenerator.GenerateSystemErrorsFile(RteFunctionsGenerator_Cpp.GetRteFolder());

            GenerateComponentsFiles();
            
            return true;
        }

       
        void GenerateDataTypesFile(String folder)
        {
            RteDataTypesGenerator_Cpp dataTypesGenerator = new RteDataTypesGenerator_Cpp();
            dataTypesGenerator.GenerateDataTypesFile(folder);
        }

        
        public void GenerateComponentsFiles()
        {
            Directory.CreateDirectory(RteFunctionsGenerator_Cpp.GetComponentsFolder());

            RteComponentGenerator_Cpp componentGenerator = new RteComponentGenerator_Cpp();
            componentGenerator.GenerateComponentsFiles();
        }
        

        void GenerateScheduler()
        {
            RteSchedulerGenerator_Cpp schedulerGenerator = new RteSchedulerGenerator_Cpp();
            schedulerGenerator.GenerateShedulerFiles(RteFunctionsGenerator_Cpp.GetRteFolder());
        }
    }
}
