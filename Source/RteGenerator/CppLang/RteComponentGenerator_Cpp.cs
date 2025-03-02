using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public class RteComponentGenerator_Cpp
    {
        public RteComponentGenerator_Cpp()
        {
        }

        public void GenerateComponentsFiles()
        {
            foreach (ApplicationSwComponentType component in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                GenerateComponent(component);
            }
        }

        void GenerateComponent(ApplicationSwComponentType component)
        {
            /* Generate component Folder */
            String componentFolder = RteFunctionsGenerator_Cpp.GetComponentsFolder() + "\\" + component.Name;
            String componentSkeletonFolder = componentFolder + "\\contracts\\skeleton";
            String rteDir = RteFunctionsGenerator_Cpp.GetRteFolder() + "\\";
            String incDir = componentSkeletonFolder + "\\include\\";
            String srcDir = componentSkeletonFolder + "\\src\\";
            Directory.CreateDirectory(incDir);
            Directory.CreateDirectory(srcDir);
            Directory.CreateDirectory(componentFolder + "\\include\\");
            Directory.CreateDirectory(componentFolder + "\\src\\");

            /* Fill sections of code */


            /* Each component's runnable shall be in its own file */
            foreach (RunnableDefenition runnable in component.Runnables)
            {
                string arguments = "";
                string returnType = "";

                String runnableDefenitionLine = RteFunctionsGenerator_Cpp.Generate_RunnableDeclaration(component, runnable, true, out returnType);
                
                CreateRunnableFile(srcDir, componentFolder + "\\src\\", component, runnable, runnableDefenitionLine, arguments, returnType);
            }


            /* Generate funcitons for Sender-Receiver ports and call operations from client ports */
            ComponentRteHeaderGenerator_Cpp.GenerateHeader(rteDir, component);

            CreateComponentIncludes(incDir, componentFolder + "\\include\\", component);
        }

        void CreateComponentIncludes(String skeletonDir, String srcDir, ApplicationSwComponentType componentDefenition)
        {
            String fileName = componentDefenition.Name + ".hpp";
            String fullFileName = skeletonDir + fileName;
            String srcFileName = srcDir + fileName;

            StreamWriter writer = new StreamWriter(fullFileName);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, fullFileName, "Implementation for header of " + componentDefenition.Name);
            RteFunctionsGenerator_Cpp.OpenCppGuardDefine(writer);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.IncludesLine);
            writer.WriteLine("");
            RteFunctionsGenerator_Cpp.AddInclude(writer, "Rte_" + componentDefenition.Name + ".hpp");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.MacrosLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.TypeDefenitionsLine);
            writer.WriteLine("");
            
            WriteComponentClass(writer, componentDefenition);
            

            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.ExternalVariablesLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfExternalVariableLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.GlobalFunctionsDeclarationLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfGlobalFunctionsDeclarationLine);
            writer.WriteLine("");

            RteFunctionsGenerator_Cpp.CloseCppGuardDefine(writer);
            writer.Close();

            if (!File.Exists(srcFileName))
            {
                File.Copy(fullFileName, srcFileName, false);
            }
        }

        void WriteComponentClass(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            String rteStructureName = RteFunctionsGenerator_Cpp.ComponentRteDataStructureDefenitionName(compDefenition);
            String baseClassName    = RteFunctionsGenerator_Cpp.ComponentBaseClassDefenitionName(compDefenition);

            writer.WriteLine("class " + compDefenition.Name + " final: private " + baseClassName);
            writer.WriteLine("{");
            writer.WriteLine("public:");

            writer.WriteLine("    /* Constructor */");
            writer.WriteLine("    " + compDefenition.Name + "(const " + rteStructureName + " &Rte);");
            writer.WriteLine("");

            writer.WriteLine("    /* Component runnables */");
            foreach (RunnableDefenition runnable in compDefenition.Runnables)
            {
                String returnType = "";
                String runnableName = RteFunctionsGenerator_Cpp.Generate_RunnableDeclaration(compDefenition, runnable, false, out returnType);
                writer.WriteLine("    " + runnableName + ";");
            }
            writer.WriteLine("");
            writer.WriteLine("protected:");
            writer.WriteLine("    /* Make other functions here */ ");
            writer.WriteLine("private:");
            writer.WriteLine("");
            writer.WriteLine("};");
            writer.WriteLine("");
        }

        void CreateRunnableFile(String skeletonDir, String srcDir, ApplicationSwComponentType compDefenition, RunnableDefenition runnable, String runnableDefenitionLine, String arguments, String returnType)
        {
            String filename = compDefenition.Name + "_ru" + runnable.Name + ".cpp";
            String fullFileName = skeletonDir + filename;
            String srcFileName = srcDir + filename; 
            StreamWriter writer = new StreamWriter(fullFileName);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, fullFileName, "Implementation for " + compDefenition.Name + "_" + runnable.Name);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.IncludesLine);
            writer.WriteLine("");
            RteFunctionsGenerator_Cpp.AddInclude(writer, compDefenition.Name + ".hpp");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.MacrosLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.TypeDefenitionsLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.VariablesLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfVariableLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.LocalFunctionsDeclarationLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfLocalFunctionsDeclarationLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.LocalFunctionsDefenitionsLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfLocalFunctionsDefenitionsLine);
            writer.WriteLine("");


            writer.WriteLine(RteFunctionsGenerator_Cpp.PrivateFunctionsDefenitionsLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfPrivateFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.ProtectedFunctionsDefenitionsLine);
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfProtectedFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.PublicFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine(runnableDefenitionLine);
            writer.WriteLine("{");
            if (returnType != "void")
            {
                writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_OK + ";");
            }
            else
            {
                writer.WriteLine("    ");
            }
            
            writer.WriteLine("}");
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfPublicFunctionsDefenitionsLine);
            writer.WriteLine("");

            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();

            if (!File.Exists(srcFileName))
            {
                File.Copy(fullFileName, srcFileName, false);
            }
        }
    }
}
