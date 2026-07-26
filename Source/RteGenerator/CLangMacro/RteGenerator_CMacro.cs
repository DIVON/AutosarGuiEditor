using System;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CMacro
{
    public class RteGenerator_CMacro
    {
        public bool Generate()
        {
            /* Create base folders */
            Directory.CreateDirectory(RteFunctionsGenerator_CMacro.GetRteFolder());
            GenerateDataTypesFile(RteFunctionsGenerator_CMacro.GetRteFolder());
            GenerateScheduler();

            RteConnectionGenerator_CMacro connectionsGenerator = new RteConnectionGenerator_CMacro();
            connectionsGenerator.GenerateConnections(RteFunctionsGenerator_CMacro.GetRteFolder());

            ReturnCodesGenerator_CMacro returnCodesGenerator = new ReturnCodesGenerator_CMacro();
            returnCodesGenerator.GenerateReturnCodesFile(RteFunctionsGenerator_CMacro.GetRteFolder());

            /* Create system errors file */
            SystemErrorGenerator_CMacro systemErrorGenerator = new SystemErrorGenerator_CMacro();
            systemErrorGenerator.GenerateSystemErrorsFile(RteFunctionsGenerator_CMacro.GetRteFolder());
            systemErrorGenerator.GenerateSystemsErrorsDescriptionFile(RteFunctionsGenerator_CMacro.GetRteFolder());

            Rte_OnBeforeAfterThreadProtectionGenerator_CMacro interruptProtectionGenerator = new Rte_OnBeforeAfterThreadProtectionGenerator_CMacro();
            interruptProtectionGenerator.GenerateThreadProtectionFunctions(RteFunctionsGenerator_CMacro.GetRteFolder());

            GenerateComponentsFiles();
            
            return true;
        }

       
        void GenerateDataTypesFile(String folder)
        {
            RteDataTypesGenerator_CMacro dataTypesGenerator = new RteDataTypesGenerator_CMacro();
            dataTypesGenerator.GenerateDataTypesFile(folder);
        }

        
        public void GenerateComponentsFiles()
        {
            Directory.CreateDirectory(RteFunctionsGenerator_CMacro.GetComponentsFolder());

            RteComponentGenerator_CMacro componentGenerator = new RteComponentGenerator_CMacro();
            componentGenerator.GenerateComponentsFiles();
        }
        

        void GenerateScheduler()
        {
            RteSchedulerGenerator_CMacro schedulerGenerator = new RteSchedulerGenerator_CMacro();
            schedulerGenerator.GenerateShedulerFiles(RteFunctionsGenerator_CMacro.GetRteFolder());
        }
    }
}
